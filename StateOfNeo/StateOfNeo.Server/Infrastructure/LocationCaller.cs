﻿using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StateOfNeo.Common;
using StateOfNeo.Data;
using StateOfNeo.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace StateOfNeo.Server.Infrastructure
{
    public class LocationCaller
    {
        private Dictionary<string, Tuple<string, string, double, double>> IPs = new Dictionary<string, Tuple<string, string, double, double>>();

        private StateOfNeoContext _ctx;

        public LocationCaller(StateOfNeoContext ctx)
        {
            _ctx = ctx;
        }

        public async Task UpdateAllNodeLocations()
        {
            var addresses = _ctx.NodeAddresses
                .Include(adr => adr.Node)
                .Where(adr => !adr.Node.Latitude.HasValue || !adr.Node.Longitude.HasValue)
                .ToList();

            foreach (var address in addresses)
            {
                var node = _ctx.Nodes
                    .FirstOrDefault(n => n.Id == address.NodeId);
                await UpdateNode(node, address.Ip);
            }
        }

        public async Task UpdateNodeLocation(int nodeId)
        {
            var node = _ctx.Nodes
                .Include(n => n.NodeAddresses)
                .FirstOrDefault(n => n.Id == nodeId);

            if (node != null && node.NodeAddresses.Count() > 0)
            {
                foreach (var address in node.NodeAddresses)
                {
                    var result = await UpdateNode(node, address.Ip);
                    if (result)
                    {
                        return;
                    }
                }
            }
        }

        private async Task<bool> UpdateNode(Node node, string ip)
        {
            try
            {
                if (node.Latitude == null || node.Longitude == null)
                {

                    var response = await CheckIpCall(ip);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseText = await response.Content.ReadAsStringAsync();
                        var responseOject = JsonConvert.DeserializeObject<LocationModel>(responseText);

                        node.FlagUrl = responseOject.Flag;
                        node.Location = responseOject.CountryName;
                        node.Latitude = responseOject.Latitude;
                        node.Longitude = responseOject.Longitude;
                        
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return false;
        }

        private async Task<HttpResponseMessage> CheckIpCall(string ip)
        {
            HttpResponseMessage response;
            try
            {
                using (var http = new HttpClient())
                {
                    var req = new HttpRequestMessage(HttpMethod.Get, $"https://api.ipdata.co/{ip}");
                    response = await http.SendAsync(req);
                }
            }
            catch (Exception e)
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            return response;
        }
    }
}
