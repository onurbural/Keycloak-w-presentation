using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Keycloak.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private static string _erisimTokeni;  
        private static List<Kullanici> _girisYapanKullaniciListesi = new List<Kullanici>();

        public AuthController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpPost("token")]
        public async Task<IActionResult> TokenAl([FromBody] GirisIstegi girisIstegi)
        {
            if (girisIstegi == null || string.IsNullOrEmpty(girisIstegi.KullaniciAdi) || string.IsNullOrEmpty(girisIstegi.Sifre))
            {
                return BadRequest("Kullanıcı adı ve şifre gerekli.");
            }

            var tokenRequest = new Dictionary<string, string>
            {
                { "client_id", "myapp2" },
                { "grant_type", "password" },
                { "username", girisIstegi.KullaniciAdi },
                { "password", girisIstegi.Sifre },
                { "client_secret", "MPEj3jObpnwUKr9joNhCEnKGMFBj9CZ7" }
            };

            var content = new FormUrlEncodedContent(tokenRequest);

            var response = await _httpClient.PostAsync("http://localhost:8080/realms/MyRealm/protocol/openid-connect/token", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<TokenDto>(responseContent);

                _erisimTokeni = tokenResponse.AccessToken;

                var existingUser = _girisYapanKullaniciListesi.FirstOrDefault(u => u.KullaniciAdi == girisIstegi.KullaniciAdi);
                if (existingUser == null)
                {
                    var user = new Kullanici
                    {
                        KullaniciAdi = girisIstegi.KullaniciAdi,
                        GirisZamani = DateTime.Now,
                        TokenGecerlilikSuresi = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn)
                    };
                    _girisYapanKullaniciListesi.Add(user);
                }
                else
                {
                    existingUser.TokenGecerlilikSuresi = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn);
                }

                return Ok(tokenResponse);
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Token alma işlemi başarısız.");
            }
        }

        [HttpGet("oturumkisi-sayisi")]
        public async Task<IActionResult> OturumdakiKisiSayisiGetir()
        {
            if (string.IsNullOrEmpty(_erisimTokeni))
            {
                return Unauthorized("Token bulunamadı. Lütfen önce giriş yapın.");
            }

            var requestUrl = "http://localhost:8080/admin/realms/MyRealm/clients/dd2f2f4c-5067-47c5-bcd7-0fbf870d608a/session-count";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_erisimTokeni}");

            var response = await _httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var sessionCount = JsonConvert.DeserializeObject<OturumdakiKisiSayisi>(responseContent);
                return Ok(sessionCount);
            }
            else
            {
                return StatusCode((int)response.StatusCode, $"Oturum sayısını alma işlemi başarısız: {response.ReasonPhrase}");
            }
        }

        [HttpPost("cikis")]
        public async Task<IActionResult> Cikis([FromBody] CikisIstegi logoutRequest)
        {
            if (string.IsNullOrEmpty(logoutRequest.RefreshToken))
            {
                return BadRequest("Refresh token gerekli.");
            }

            var logoutContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", "myapp2"),
                new KeyValuePair<string, string>("client_secret", "MPEj3jObpnwUKr9joNhCEnKGMFBj9CZ7"),
                new KeyValuePair<string, string>("refresh_token", logoutRequest.RefreshToken)
            });

            var response = await _httpClient.PostAsync("http://localhost:8080/realms/MyRealm/protocol/openid-connect/logout", logoutContent);

            if (response.IsSuccessStatusCode)
            {
                _erisimTokeni = null; 
                _girisYapanKullaniciListesi.RemoveAll(u => u.KullaniciAdi == logoutRequest.KullaniciAdi);  

                return Ok("Çıkış başarılı.");
            }
            else
            {
                return StatusCode((int)response.StatusCode, $"Çıkış başarısız: {response.ReasonPhrase}");
            }
        }
        [HttpGet("giris-yapan-kullanicilar")]
        public IActionResult GirisYapmisKullanicilar()
        {
            _girisYapanKullaniciListesi.RemoveAll(u => u.TokenGecerlilikSuresi <= DateTime.Now);

            var result = new
            {
                Count = _girisYapanKullaniciListesi.Count, 
                Users = _girisYapanKullaniciListesi        
            };

            return Ok(result);
        }

    }

    // Kullanıcı Modeli
    public class Kullanici
    {
        public string KullaniciAdi { get; set; }
        public DateTime GirisZamani { get; set; }
        public DateTime TokenGecerlilikSuresi { get; set; }  
    }

    // Çıkış yap için DTO
    public class CikisIstegi
    {
        public string KullaniciAdi { get; set; }
        public string RefreshToken { get; set; }
    }

    //Authentication isteği için DTO
    public class GirisIstegi
    {
        public string KullaniciAdi { get; set; }
        public string Sifre { get; set; }
    }

    //Token response için DTO
    public class TokenDto
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }

    // Oturum sayısı için Dto
    public class OturumdakiKisiSayisi
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
