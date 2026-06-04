using Identity_Auth_MicroService.Servives_Abstraction.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Identity_Auth_MicroService.Services.Services
{
    public class ClinicClient : IClinicClient
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public ClinicClient(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
            _http.DefaultRequestHeaders.Add("X-Internal-Key", _config["Internal:Key"]);
        }

        public async Task<bool> IsDoctorActiveAsync(string identityUserId)
        {
            var res = await _http.GetAsync($"/doctors/internal/is-active/{identityUserId}");
            if (!res.IsSuccessStatusCode) return false;

            var body = await res.Content.ReadFromJsonAsync<IsActiveResponse>();
            return body?.IsActive ?? false;
        }

        private record IsActiveResponse(bool IsActive);
    }
}
