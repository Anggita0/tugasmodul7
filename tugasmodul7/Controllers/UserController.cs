using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using tugasmodul7.Database;
using tugasmodul7.Models;

namespace tugasmodul7.Controllers
{
    [ApiController]
    [Route("user")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;

        public UserController(AppDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        [HttpPost("sync/{id}")]
        public async Task<IActionResult> SyncUser(int id)
        {
            var response = await _httpClient.GetAsync($"https://dummy-user-tan.vercel.app/user/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound("Gagal ambil data dari API eksternal");

            var json = await response.Content.ReadAsStringAsync();
            var apiUser = JsonSerializer.Deserialize<UserDetailAPI>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (apiUser == null)
                return BadRequest("Deserialisasi gagal");

            var user = await _context.Users.FindAsync(apiUser.Id);
            if (user == null)
            {
                _context.Users.Add(new User
                {
                    Id = apiUser.Id,
                    Name = apiUser.Name,
                });
            }

            var finance = await _context.UserFinances.FindAsync(apiUser.Id);
            if (finance == null)
            {
                _context.UserFinances.Add(new UserFinance
                {
                    Id = apiUser.Id,
                    Saldo = apiUser.Saldo,
                    Hutang = apiUser.Hutang
                });
            }
            else
            {
                finance.Saldo = apiUser.Saldo;
                finance.Hutang = apiUser.Hutang;
                _context.UserFinances.Update(finance);
            }

            await _context.SaveChangesAsync();
            return Ok("Sinkronisasi berhasil.");
        }

        // ✅ 2. Endpoint Ambil Data Gabungan (User + Saldo & Hutang)
        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetUserDetail(int id)
        {
            var user = await _context.Users.FindAsync(id);
            var finance = await _context.UserFinances.FindAsync(id);

            if (user == null || finance == null)
                return NotFound("User atau detail tidak ditemukan");

            var result = new UserDetail
            {
                Id = user.Id,
                Name = user.Name,
                Age = user.Age,
                UserWithDetail = new UserWithDetail
                {
                    Saldo = finance.Saldo,
                    Hutang = finance.Hutang
                }
            };

            return Ok(result);
        }
    }
}
