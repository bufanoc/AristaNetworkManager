using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AristaNetworkManager.Services
{
    public class ApiService : IDisposable
    {
        private readonly HttpClient _client;
        private readonly string _username;
        private string _password;
        private bool _disposed;
        private readonly int _timeout;

        public ApiService(string username, string password)
        {
            var config = ConfigurationService.Instance.Settings.ApiSettings;
            _username = username;
            _password = password;
            _timeout = config.ConnectionTimeout;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => 
                    config.IgnoreSslErrors
            };

            _client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(_timeout)
            };

            UpdateAuthenticationHeader();
        }

        private void UpdateAuthenticationHeader()
        {
            var authString = $"{_username}:{_password}";
            var base64Auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(authString));
            _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", base64Auth);
        }

        public async Task<bool> TestConnectionAsync(string ipAddress)
        {
            var request = new
            {
                jsonrpc = "2.0",
                method = "runCmds",
                @params = new
                {
                    version = 1,
                    cmds = new[] { "show version" }
                },
                id = "1"
            };

            var response = await SendEapiRequestAsync(ipAddress, request);
            return response != null && !response.ContainsKey("error");
        }

        public async Task<JObject> GetSwitchConfigAsync(string ipAddress)
        {
            var request = new
            {
                jsonrpc = "2.0",
                method = "runCmds",
                @params = new
                {
                    version = 1,
                    cmds = new[] { "enable", "show running-config" }
                },
                id = "1"
            };

            var response = await SendEapiRequestAsync(ipAddress, request);
            if (response?["result"] != null && response["result"].HasValues)
            {
                // Get the last command's result (show running-config)
                return new JObject { ["result"] = new JArray { response["result"]![1] } };
            }
            return null;
        }

        public async Task<Dictionary<string, string>> GetPhysicalInterfacesAsync(string ipAddress)
        {
            var request = new
            {
                jsonrpc = "2.0",
                method = "runCmds",
                @params = new
                {
                    version = 1,
                    cmds = new[] { "enable", "show interfaces status" }
                },
                id = "1"
            };

            var interfaces = new Dictionary<string, string>();
            var response = await SendEapiRequestAsync(ipAddress, request);
            
            if (response?["result"] != null && response["result"].HasValues)
            {
                var interfacesData = response["result"]![1]!["interfaceStatuses"];
                foreach (JProperty prop in interfacesData.Children<JProperty>())
                {
                    var status = interfacesData[prop.Name];
                    if (status["linkStatus"]?.ToString() == "connected")
                    {
                        interfaces[prop.Name] = status["interfaceType"]?.ToString();
                    }
                }
            }

            return interfaces;
        }

        public async Task<Dictionary<string, string>> GetVlansAsync(string ipAddress)
        {
            var request = new
            {
                jsonrpc = "2.0",
                method = "runCmds",
                @params = new
                {
                    version = 1,
                    cmds = new[] { "enable", "show vlan" }
                },
                id = "1"
            };

            var vlans = new Dictionary<string, string>();
            var response = await SendEapiRequestAsync(ipAddress, request);
            
            if (response?["result"] != null && response["result"].HasValues)
            {
                var vlansData = response["result"]![1]!["vlans"];
                foreach (JProperty prop in vlansData.Children<JProperty>())
                {
                    var vlan = vlansData[prop.Name];
                    vlans[prop.Name] = vlan["name"]?.ToString();
                }
            }

            return vlans;
        }

        public async Task<List<(string LocalInterface, string RemoteDevice, string RemoteInterface)>> GetLldpNeighborsAsync(string ipAddress)
        {
            var request = new
            {
                jsonrpc = "2.0",
                method = "runCmds",
                @params = new
                {
                    version = 1,
                    cmds = new[] { "enable", "show lldp neighbors" }
                },
                id = "1"
            };

            var neighbors = new List<(string, string, string)>();
            var response = await SendEapiRequestAsync(ipAddress, request);
            
            if (response?["result"] != null && response["result"].HasValues)
            {
                var lldpData = response["result"]![1]!["lldpNeighbors"];
                foreach (var neighbor in lldpData)
                {
                    var localInterface = neighbor["port"]?.ToString();
                    var remoteDevice = neighbor["neighborDevice"]?.ToString();
                    var remoteInterface = neighbor["neighborPort"]?.ToString();
                    
                    if (!string.IsNullOrEmpty(localInterface) && 
                        !string.IsNullOrEmpty(remoteDevice) && 
                        !string.IsNullOrEmpty(remoteInterface))
                    {
                        neighbors.Add((localInterface, remoteDevice, remoteInterface));
                    }
                }
            }

            return neighbors;
        }

        public async Task<List<(string VtepIp, int Vni, string[] Interfaces)>> GetVxlanConfigAsync(string ipAddress)
        {
            var request = new
            {
                jsonrpc = "2.0",
                method = "runCmds",
                @params = new
                {
                    version = 1,
                    cmds = new[] { "enable", "show vxlan vni" }
                },
                id = "1"
            };

            var vxlanInfo = new List<(string, int, string[])>();
            var response = await SendEapiRequestAsync(ipAddress, request);
            
            if (response?["result"] != null && response["result"].HasValues)
            {
                var vxlanData = response["result"]![1]!["vnis"];
                foreach (JProperty prop in vxlanData.Children<JProperty>())
                {
                    var vni = int.Parse(prop.Name);
                    var data = vxlanData[prop.Name];
                    var vtepIp = data["vtepIp"]?.ToString();
                    var interfaces = data["interfaces"]?.ToObject<string[]>() ?? Array.Empty<string>();
                    
                    if (!string.IsNullOrEmpty(vtepIp))
                    {
                        vxlanInfo.Add((vtepIp, vni, interfaces));
                    }
                }
            }

            return vxlanInfo;
        }

        public async Task<bool> UpdateSwitchConfigAsync(string ipAddress, string newConfig)
        {
            var request = new
            {
                jsonrpc = "2.0",
                method = "runCmds",
                @params = new
                {
                    version = 1,
                    cmds = new[] { "enable", "configure", newConfig }
                },
                id = "1"
            };

            var response = await SendEapiRequestAsync(ipAddress, request);
            return response != null && !response.ContainsKey("error");
        }

        public async Task<bool> UpdateSwitchPasswordAsync(string ipAddress, string newPassword)
        {
            try
            {
                var request = new
                {
                    jsonrpc = "2.0",
                    method = "runCmds",
                    @params = new
                    {
                        version = 1,
                        cmds = new[]
                        {
                            "enable",
                            "configure",
                            $"username {_username} secret {newPassword}",
                            "end"
                        }
                    },
                    id = "1"
                };

                var response = await SendEapiRequestAsync(ipAddress, request);
                if (response != null)
                {
                    // Update the client's password for future requests
                    _password = newPassword;
                    UpdateAuthenticationHeader();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<SwitchDetails> GetSwitchDetailsAsync(string ipAddress)
        {
            try
            {
                var request = new
                {
                    jsonrpc = "2.0",
                    method = "runCmds",
                    @params = new
                    {
                        version = 1,
                        cmds = new[] { "show version" }
                    },
                    id = "1"
                };

                var response = await SendEapiRequestAsync(ipAddress, request);
                if (response?["result"]?[0] != null)
                {
                    var result = response["result"][0];
                    return new SwitchDetails
                    {
                        Version = result["version"]?.ToString() ?? "",
                        Model = result["modelName"]?.ToString() ?? "",
                        SerialNumber = result["serialNumber"]?.ToString() ?? "",
                        SystemMacAddress = result["systemMacAddress"]?.ToString() ?? "",
                        Uptime = result["uptime"]?.ToString() ?? ""
                    };
                }
                return new SwitchDetails();
            }
            catch (Exception)
            {
                return new SwitchDetails();
            }
        }

        public async Task<string> GetSwitchHostnameAsync(string ipAddress)
        {
            try
            {
                var url = $"https://{ipAddress}/command-api";
                var request = new
                {
                    jsonrpc = "2.0",
                    method = "runCmds",
                    @params = new
                    {
                        version = 1,
                        cmds = new[] { "show hostname" }
                    },
                    id = "1"
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _client.PostAsync(url, content);
                var jsonResponse = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JObject.Parse(jsonResponse);
                    return result["result"]?[0]?["hostname"]?.ToString() ?? ipAddress;
                }
                return ipAddress;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching hostname: {ex.Message}");
                return ipAddress;
            }
        }

        private async Task<JObject> SendEapiRequestAsync(string ipAddress, object request)
        {
            try
            {
                var url = $"https://{ipAddress}/command-api";
                var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                
                Console.WriteLine($"Sending request to {url}");
                var requestContent = await content.ReadAsStringAsync();
                Console.WriteLine($"Request content: {requestContent}");
                
                var response = await _client.PostAsync(url, content);
                
                // Read the response content even if it's not successful
                var jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response status: {response.StatusCode}");
                Console.WriteLine($"Response from switch: {jsonResponse}");
                
                response.EnsureSuccessStatusCode();
                var result = JObject.Parse(jsonResponse);
                
                // Check for API-level errors
                if (result.ContainsKey("error"))
                {
                    Console.WriteLine($"API Error: {result["error"]!["message"]}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error communicating with switch: {ex.Message}");
                if (ex is HttpRequestException)
                {
                    Console.WriteLine($"HTTP Request Error: {ex.InnerException?.Message}");
                }
                return null;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _client.Dispose();
                _disposed = true;
            }
        }
    }

    public class SwitchDetails
    {
        public string Version { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public string SystemMacAddress { get; set; }
        public string Uptime { get; set; }
    }
}
