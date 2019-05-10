﻿using System;
using System.Collections.Generic;
using System.IO;
using static LiteDB.Constants;

namespace LiteDB.Engine
{
    internal class DataService
    {
        /// <summary>
        /// Get maximum data bytes[] that fit in 1 page = 8149
        /// </summary>
        public const int MAX_DATA_BYTES_PER_PAGE =
            PAGE_SIZE - // 8192
            PAGE_HEADER_SIZE - // [32 bytes]
            BasePage.SLOT_SIZE - // [4 bytes]
            DataBlock.DATA_BLOCK_FIXED_SIZE; // [6 bytes];

        private Snapshot _snapshot;

        public DataService(Snapshot snapshot)
        {
            _snapshot = snapshot;
        }

        /// <summary>
        /// Insert BsonDocument into new data pages
        /// </summary>
        public PageAddress Insert(BsonDocument doc)
        {
            var bytesLeft = doc.GetBytesCount(true);

            if (bytesLeft > MAX_DOCUMENT_SIZE) throw new LiteException(0, "Document size exceed {0} limit", MAX_DOCUMENT_SIZE);

            var firstBlock = PageAddress.Empty;

            IEnumerable<BufferSlice> source()
            {
                var blockIndex = 0;
                DataBlock lastBlock = null;

                while (bytesLeft > 0)
                {
                    var bytesToCopy = Math.Min(bytesLeft, MAX_DATA_BYTES_PER_PAGE);
                    var dataPage = _snapshot.GetFreePage<DataPage>(bytesToCopy + DataBlock.DATA_BLOCK_FIXED_SIZE);
                    var dataBlock = dataPage.InsertBlock(bytesToCopy, blockIndex++ > 0);

                    if (lastBlock != null)
                    {
                        lastBlock.SetNextBlock(dataBlock.Position);
                    }

                    if (firstBlock.IsEmpty) firstBlock = dataBlock.Position;

                    yield return dataBlock.Buffer;

                    lastBlock = dataBlock;

                    bytesLeft -= bytesToCopy;
                }
            }

            // consume all source bytes to write BsonDocument direct into PageBuffer
            // must be fastest as possible
            using (var w = new BufferWriter(source()))
            {
                // already bytes count calculate at method start
                w.WriteDocument(doc, false);
                w.Consume();
            }

            return firstBlock;
        }

        /// <summary>
        /// Update document using same page position as reference
        /// </summary>
        public void Update(CollectionPage col, PageAddress blockAddress, BsonDocument doc)
        {
            var bytesLeft = doc.GetBytesCount(true);

            if (bytesLeft > MAX_DOCUMENT_SIZE) throw new LiteException(0, "Document size exceed {0} limit", MAX_DOCUMENT_SIZE);

            DataBlock lastBlock = null;
            var updateAddress = blockAddress;

            IEnumerable <BufferSlice> source()
            {
                var bytesToCopy = 0;

                while (bytesLeft > 0)
                {
                    // if last block contains new block sequence, continue updating
                    if (updateAddress.IsEmpty == false)
                    {
                        var dataPage = _snapshot.GetPage<DataPage>(updateAddress.PageID);
                        var currentBlock = dataPage.GetBlock(updateAddress.Index);

                        // try get full page size content
                        bytesToCopy = Math.Min(bytesLeft, dataPage.FreeBytes + currentBlock.Buffer.Count);

                        // get current free slot linked list
                        var slot = BasePage.FreeIndexSlot(dataPage.FreeBytes);

                        var updateBlock = dataPage.UpdateBlock(currentBlock, bytesToCopy);

                        _snapshot.AddOrRemoveFreeList(dataPage, slot);

                        yield return updateBlock.Buffer;

                        lastBlock = updateBlock;

                        // go to next address (if extits)
                        updateAddress = updateBlock.NextBlock;
                    }
                    else
                    {
                        bytesToCopy = Math.Min(bytesLeft, MAX_DATA_BYTES_PER_PAGE);
                        var dataPage = _snapshot.GetFreePage<DataPage>(bytesToCopy + DataBlock.DATA_BLOCK_FIXED_SIZE);
                        var insertBlock = dataPage.InsertBlock(bytesToCopy, true);

                        if (lastBlock != null)
                        {
                            lastBlock.SetNextBlock(insertBlock.Position);
                        }

                        yield return insertBlock.Buffer;

                        lastBlock = insertBlock;
                    }

                    bytesLeft -= bytesToCopy;
                }

                // old document was bigger than current, must delete extend blocks
                if (lastBlock.NextBlock.IsEmpty == false)
                {
                    this.Delete(lastBlock.NextBlock);
                }
            }

            // consume all source bytes to write BsonDocument direct into PageBuffer
            // must be fastest as possible
            using (var w = new BufferWriter(source()))
            {
                // already bytes count calculate at method start
                w.WriteDocument(doc, false);
                w.Consume();
            }
        }

        /// <summary>
        /// Get all buffer slices that address block contains. Need use BufferReader to read document
        /// </summary>
        public IEnumerable<BufferSlice> Read(PageAddress address)
        {
            while (address != PageAddress.Empty)
            {
                var dataPage = _snapshot.GetPage<DataPage>(address.PageID);

                var block = dataPage.GetBlock(address.Index);

                yield return block.Buffer;

                address = block.NextBlock;
            }
        }

        /// <summary>
        /// Delete all datablock that contains a document (can use multiples data blocks)
        /// </summary>
        public void Delete(PageAddress blockAddress)
        {
            // delete all document blocks
            while(blockAddress != PageAddress.Empty)
            {
                var page = _snapshot.GetPage<DataPage>(blockAddress.PageID);
                var block = page.GetBlock(blockAddress.Index);
                var slot = BasePage.FreeIndexSlot(page.FreeBytes);

                // delete block inside page
                page.DeleteBlock(blockAddress.Index);

                // fix page empty list (or delete page)
                _snapshot.AddOrRemoveFreeList(page, slot);

                blockAddress = block.NextBlock;
            }
        }

        /// <summary>
        /// Read all documents from current collection with NO index use - read direct from free lists
        /// There is no document order
        /// </summary>
        public IEnumerable<BsonDocument> ReadAll(HashSet<string> fields = null)
        {
            for (var slot = 0; slot < CollectionPage.PAGE_FREE_LIST_SLOTS; slot++)
            {
                var next = _snapshot.CollectionPage.FreeDataPageID[slot];

                while (next != uint.MaxValue)
                {
                    var page = _snapshot.GetPage<DataPage>(next);

                    foreach (var block in page.GetBlocks(true))
                    {
                        using (var r = new BufferReader(this.Read(block)))
                        {
                            var doc = r.ReadDocument(fields);

                            yield return doc;
                        }
                    }

                    next = page.NextPageID;
                }
            }
        }
    }
}