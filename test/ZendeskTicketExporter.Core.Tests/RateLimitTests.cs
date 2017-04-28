using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ZendeskTicketExporter.Core.Tests
{
    public class RateLimitTests
    {
        private readonly Random _rand;

        public RateLimitTests()
        {
            _rand = new Random();
        }

        [Fact]
        public void GetMultipleTicketsAsync_one_ticket()
        {
            var api = new ZendeskApi_v2.ZendeskApi(/* add login info */);

            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 8
            };
            Parallel.For(0, 1000, options, i =>
            {
                var batch = api.Tickets.GetMultipleTicketsAsync(new List<long>() { 12345 }).Result;

                if (batch.Count == 0 || batch.Tickets.Count == 0)
                    throw new ApplicationException("Hit rate limit.");
            });
        }

        [Fact]
        public void GetMultipleTicketsAsync_two_tickets()
        {
            var api = new ZendeskApi_v2.ZendeskApi(/* add login info */);

            var options = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 16
            };
            Parallel.For(0, 1000, options, i =>
            {
                var batch = api.Tickets.GetMultipleTicketsAsync(new List<long>() { 12345, 12346 }).Result;

                if (batch.Count == 0 || batch.Tickets.Count == 0)
                    throw new ApplicationException("Hit rate limit.");
            });
        }

        [Fact]
        public void GetMultipleTickets_one_ticket()
        {
            var api = new ZendeskApi_v2.ZendeskApi(/* add login info */);

            var batch = api.Tickets.GetMultipleTickets(new List<long>() { 12345 });

            if (batch.Count == 0 || batch.Tickets.Count == 0)
                throw new ApplicationException("Hit rate limit.");
        }

        [Fact]
        public void GetMultipleTickets_two_tickets()
        {
            var api = new ZendeskApi_v2.ZendeskApi(/* add login info */);

            for (var i = 0; i < 1000; i++)
            {
                var batch = api.Tickets.GetMultipleTickets(new List<long>() { 12345, 12346 });

                if (batch.Count == 0 || batch.Tickets.Count == 0)
                    throw new ApplicationException("Hit rate limit.");
            }
        }
    }
}