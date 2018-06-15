﻿using Microsoft.EntityFrameworkCore;
using StateOfNeo.Data.Models;
using System;

namespace StateOfNeo.Data
{
    public class StateOfNeoContext : DbContext
    {
        public StateOfNeoContext(DbContextOptions<StateOfNeoContext> options)
            : base(options)
        {

        }

        public DbSet<BlockchainInfo> BlockchainInfos { get; set; }
        public DbSet<Node> Nodes { get; set; }
        public DbSet<NodeAddress> NodeAddresses { get; set; }
        public DbSet<TimeEvent> TimeEvents { get; set; }
    }
}
