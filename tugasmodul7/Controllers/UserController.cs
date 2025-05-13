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
    [Route("[controller]")]
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


        [HttpGet("sync/{id}")]
        public async Task<IActionResult> SyncUser(int id)
        {
            var response = await _httpClient.GetAsync($"https://dummy-user-tan.vercel.app/user/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound("Gagal mengambil data dari API eksternal");

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<User>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser != null)
            {
                existingUser.Name = user.Name;
                existingUser.Saldo = user.Saldo;
                existingUser.Hutang = user.Hutang;
            }
            else
            {
                _context.Users.Add(user);
            }

            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetUserDetail(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("Data tidak ditemukan di database lokal");

            return Ok(user);
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAllUsers()
        //{
        //    var users = await _context.Users.ToListAsync();
        //    Console.WriteLine($"Jumlah user di DB: {users.Count}");
        //    return Ok(users);
        //}


        //[HttpGet("sync/all")]
        //public async Task<IActionResult> SyncAllUsers()
        //{
        //    var response = await _httpClient.GetAsync("https://dummy-user-tan.vercel.app/user/");
        //    if (!response.IsSuccessStatusCode)
        //    {
        //        return StatusCode((int)response.StatusCode, "Gagal mengambil data dari API eksternal");
        //    }

        //    var json = await response.Content.ReadAsStringAsync();
        //    var users = JsonSerializer.Deserialize<List<User>>(json, new JsonSerializerOptions
        //    {
        //        PropertyNameCaseInsensitive = true
        //    });

        //    foreach (var user in users)
        //    {
        //        var existingUser = await _context.Users.FindAsync(user.Id);
        //        if (existingUser != null)
        //        {
        //            existingUser.Name = user.Name;
        //            existingUser.Saldo = user.Saldo;
        //            existingUser.Hutang = user.Hutang;
        //        }
        //        else
        //        {
        //            _context.Users.Add(user);
        //        }
        //    }

        //    await _context.SaveChangesAsync();

        //    return Ok(new
        //    {
        //        message = "Sinkronisasi semua user berhasil",
        //        totalUser = users.Count
        //    });
        //}

    }
}

