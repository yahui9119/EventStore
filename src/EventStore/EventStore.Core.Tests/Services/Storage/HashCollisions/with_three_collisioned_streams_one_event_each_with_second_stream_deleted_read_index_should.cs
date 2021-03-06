// Copyright (c) 2012, Event Store LLP
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
// 
// Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
// Neither the name of the Event Store LLP nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 

using System.Linq;
using EventStore.Core.Data;
using EventStore.Core.Services.Storage.ReaderIndex;
using NUnit.Framework;
using ReadStreamResult = EventStore.Core.Services.Storage.ReaderIndex.ReadStreamResult;

namespace EventStore.Core.Tests.Services.Storage.HashCollisions
{
    [TestFixture]
    public class with_three_collisioned_streams_one_event_each_with_second_stream_deleted_read_index_should : ReadIndexTestScenario
    {
        private EventRecord _prepare1;
        private EventRecord _prepare2;
        private EventRecord _delete2;
        private EventRecord _prepare3;

        protected override void WriteTestScenario()
        {
            _prepare1 = WriteSingleEvent("AB", 0, "test1");

            _prepare2 = WriteSingleEvent("CD", 0, "test2");
            _delete2 = WriteDelete("CD");

            _prepare3 = WriteSingleEvent("EF", 0, "test3");
        }

        [Test]
        public void return_correct_last_event_version_for_first_stream()
        {
            Assert.AreEqual(0, ReadIndex.GetStreamLastEventNumber("AB"));
        }

        [Test]
        public void return_correct_log_record_for_first_stream()
        {
            var result = ReadIndex.ReadEvent("AB", 0);
            Assert.AreEqual(ReadEventResult.Success, result.Result);
            Assert.AreEqual(_prepare1, result.Record);
        }

        [Test]
        public void not_find_record_with_index_1_in_first_stream()
        {
            var result = ReadIndex.ReadEvent("AB", 1);
            Assert.AreEqual(ReadEventResult.NotFound, result.Result);
            Assert.IsNull(result.Record);
        }

        [Test]
        public void return_correct_range_on_from_start_range_query_for_first_stream()
        {
            var result = ReadIndex.ReadStreamEventsForward("AB", 0, 1);
            Assert.AreEqual(ReadStreamResult.Success, result.Result);
            Assert.AreEqual(1, result.Records.Length);
            Assert.AreEqual(_prepare1, result.Records[0]);
        }

        [Test]
        public void return_correct_range_on_from_end_range_query_for_first_stream()
        {
            var result = ReadIndex.ReadStreamEventsBackward("AB", 0, 1);
            Assert.AreEqual(ReadStreamResult.Success, result.Result);
            Assert.AreEqual(1, result.Records.Length);
            Assert.AreEqual(_prepare1, result.Records[0]);
        }

        [Test]
        public void return_correct_last_event_version_for_second_stream()
        {
            Assert.AreEqual(EventNumber.DeletedStream, ReadIndex.GetStreamLastEventNumber("CD"));
        }

        [Test]
        public void not_find_log_record_for_second_stream()
        {
            var result = ReadIndex.ReadEvent("CD", 0);
            Assert.AreEqual(ReadEventResult.StreamDeleted, result.Result);
            Assert.IsNull(result.Record);
        }

        [Test]
        public void not_find_record_with_index_1_in_second_stream()
        {
            var result = ReadIndex.ReadEvent("CD", 1);
            Assert.AreEqual(ReadEventResult.StreamDeleted, result.Result);
            Assert.IsNull(result.Record);
        }

        [Test]
        public void return_empty_range_on_from_start_range_query_for_second_stream()
        {
            var result = ReadIndex.ReadStreamEventsForward("CD", 0, 1);
            Assert.AreEqual(ReadStreamResult.StreamDeleted, result.Result);
        }

        [Test]
        public void return_empty_range_on_from_end_range_query_for_second_stream()
        {
            var result = ReadIndex.ReadStreamEventsBackward("CD", 0, 1);
            Assert.AreEqual(ReadStreamResult.StreamDeleted, result.Result);
        }

        [Test]
        public void return_correct_last_event_version_for_third_stream()
        {
            Assert.AreEqual(0, ReadIndex.GetStreamLastEventNumber("EF"));
        }

        [Test]
        public void return_correct_log_record_for_third_stream()
        {
            var result = ReadIndex.ReadEvent("EF", 0);
            Assert.AreEqual(ReadEventResult.Success, result.Result);
            Assert.AreEqual(_prepare3, result.Record);
        }

        [Test]
        public void not_find_record_with_index_1_in_third_stream()
        {
            var result = ReadIndex.ReadEvent("EF", 1);
            Assert.AreEqual(ReadEventResult.NotFound, result.Result);
            Assert.IsNull(result.Record);
        }

        [Test]
        public void return_correct_range_on_from_start_range_query_for_third_stream()
        {
            var result = ReadIndex.ReadStreamEventsForward("EF", 0, 1);
            Assert.AreEqual(ReadStreamResult.Success, result.Result);
            Assert.AreEqual(1, result.Records.Length);
            Assert.AreEqual(_prepare3, result.Records[0]);
        }

        [Test]
        public void return_correct_range_on_from_end_range_query_for_third_stream()
        {
            var result = ReadIndex.ReadStreamEventsBackward("EF", 0, 1);
            Assert.AreEqual(ReadStreamResult.Success, result.Result);
            Assert.AreEqual(1, result.Records.Length);
            Assert.AreEqual(_prepare3, result.Records[0]);
        }

        [Test]
        public void return_empty_range_when_asked_to_get_few_events_from_start_starting_from_1_in_first_stream()
        {
            var result = ReadIndex.ReadStreamEventsForward("AB", 1, 1);
            Assert.AreEqual(ReadStreamResult.Success, result.Result);
            Assert.AreEqual(0, result.Records.Length);
        }

        [Test]
        public void return_empty_range_when_asked_to_get_few_events_from_start_starting_from_1_in_second_stream()
        {
            var result = ReadIndex.ReadStreamEventsForward("CD", 1, 1);
            Assert.AreEqual(ReadStreamResult.StreamDeleted, result.Result);
            Assert.AreEqual(0, result.Records.Length);
        }

        [Test]
        public void return_empty_range_when_asked_to_get_few_events_from_start_starting_from_1_in_third_stream()
        {
            var result = ReadIndex.ReadStreamEventsForward("EF", 1, 1);
            Assert.AreEqual(ReadStreamResult.Success, result.Result);
            Assert.AreEqual(0, result.Records.Length);
        }

        [Test]
        public void return_empty_range_when_asked_to_get_few_events_from_end_starting_from_1_in_first_stream()
        {
            var result = ReadIndex.ReadStreamEventsBackward("AB", 1, 1);
            Assert.AreEqual(ReadStreamResult.Success, result.Result);
            Assert.AreEqual(0, result.Records.Length);
        }

        [Test]
        public void return_empty_range_when_asked_to_get_few_events_from_end_starting_from_1_in_second_stream()
        {
            var result = ReadIndex.ReadStreamEventsBackward("CD", 1, 1);
            Assert.AreEqual(ReadStreamResult.StreamDeleted, result.Result);
            Assert.AreEqual(0, result.Records.Length);
        }

        [Test]
        public void return_empty_range_when_asked_to_get_few_events_from_end_starting_from_1_in_third_stream()
        {
            var result = ReadIndex.ReadStreamEventsBackward("EF", 1, 1);
            Assert.AreEqual(ReadStreamResult.Success, result.Result);
            Assert.AreEqual(0, result.Records.Length);
        }

        [Test]
        public void return_correct_last_event_version_for_nonexistent_stream_with_same_hash()
        {
            Assert.AreEqual(-1, ReadIndex.GetStreamLastEventNumber("ZZ"));
        }

        [Test]
        public void not_find_log_record_for_nonexistent_stream_with_same_hash()
        {
            var result = ReadIndex.ReadEvent("ZZ", 0);
            Assert.AreEqual(ReadEventResult.NoStream, result.Result);
            Assert.IsNull(result.Record);
        }

        [Test]
        public void return_empty_range_on_from_start_range_query_for_non_existing_stream_with_same_hash()
        {
            var result = ReadIndex.ReadStreamEventsForward("ZZ", 0, 1);
            Assert.AreEqual(ReadStreamResult.NoStream, result.Result);
            Assert.AreEqual(0, result.Records.Length);
        }

        [Test]
        public void return_empty_range_on_from_end_range_query_for_non_existing_stream_with_same_hash()
        {
            var result = ReadIndex.ReadStreamEventsBackward("ZZ", 0, 1);
            Assert.AreEqual(ReadStreamResult.NoStream, result.Result);
            Assert.AreEqual(0, result.Records.Length);
        }

        [Test]
        public void return_all_events_on_read_all_forward()
        {
            var events = ReadIndex.ReadAllEventsForward(new TFPos(0, 0), 100).Records.Select(r => r.Event).ToArray();
            Assert.AreEqual(4, events.Length);
            Assert.AreEqual(_prepare1, events[0]);
            Assert.AreEqual(_prepare2, events[1]);
            Assert.AreEqual(_delete2, events[2]);
            Assert.AreEqual(_prepare3, events[3]);
        }

        [Test]
        public void return_all_events_on_read_all_backward()
        {
            var events = ReadIndex.ReadAllEventsBackward(GetBackwardReadPos(), 100).Records.Select(r => r.Event).ToArray();
            Assert.AreEqual(4, events.Length);
            Assert.AreEqual(_prepare1, events[3]);
            Assert.AreEqual(_prepare2, events[2]);
            Assert.AreEqual(_delete2, events[1]);
            Assert.AreEqual(_prepare3, events[0]);
        }
    }
}