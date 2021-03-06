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

using System;
using EventStore.Projections.Core.Services.Processing;
using NUnit.Framework;

namespace EventStore.Projections.Core.Tests.Services.partition_state_cache
{
    [TestFixture]
    public class when_unlocking_and_forgetting_part_of_cached_states
    {
        private PartitionStateCache _cache;
        private CheckpointTag _cachedAtCheckpointTag1;
        private CheckpointTag _cachedAtCheckpointTag2;
        private CheckpointTag _cachedAtCheckpointTag3;

        [SetUp]
        public void setup()
        {
            //given
            _cache = new PartitionStateCache();
            _cachedAtCheckpointTag1 = CheckpointTag.FromPosition(0, 1000, 900);
            _cachedAtCheckpointTag2 = CheckpointTag.FromPosition(0, 1200, 1100);
            _cachedAtCheckpointTag3 = CheckpointTag.FromPosition(0, 1400, 1300);
            _cache.CacheAndLockPartitionState(
                "partition1", new PartitionState("data1", null, _cachedAtCheckpointTag1), _cachedAtCheckpointTag1);
            _cache.CacheAndLockPartitionState(
                "partition2", new PartitionState("data2", null, _cachedAtCheckpointTag2), _cachedAtCheckpointTag2);
            _cache.CacheAndLockPartitionState(
                "partition3", new PartitionState("data3", null, _cachedAtCheckpointTag3), _cachedAtCheckpointTag3);
            // when
            _cache.Unlock(_cachedAtCheckpointTag2, forgetUnlocked: true);
        }

        [Test, ExpectedException(typeof (InvalidOperationException))]
        public void partitions_locked_before_the_unlock_position_cannot_be_retrieved_as_locked()
        {
            _cache.GetLockedPartitionState("partition1");
        }

        [Test]
        public void partitions_locked_before_the_unlock_position_cannot_be_retrieved_and_relocked_at_later_position()
        {
            var data = _cache.TryGetAndLockPartitionState("partition1", CheckpointTag.FromPosition(0, 1600, 1500));
            Assert.IsNull(data);
        }


    }
}
