using EfCoreBulkInsertOptimization.Data;
using EfCoreBulkInsertOptimization.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EfCoreBulkInsertOptimization.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EfCoreTestController : ControllerBase
    {
        private readonly ILogger<EfCoreTestController> _logger;
        private readonly AppDbContext _dbContext;

        public EfCoreTestController(ILogger<EfCoreTestController> logger, AppDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
            _dbContext.Database.EnsureCreated();
        }

        [HttpPost("InsertNormal")]
        public ActionResult<BenchmarkResult> InsertNormal()
        {
            var customers = GenerateCustomers(5000);
            var clockSaveChanges = new Stopwatch();

            //normal insert
            _dbContext.Customers.AddRange(customers);
            clockSaveChanges.Start();
            _dbContext.SaveChanges();
            clockSaveChanges.Stop();

            return Ok(new BenchmarkResult
            {
                Action = "EF Core SaveChanges",
                Entities = customers.Count,
                TimeElapsed = $"{clockSaveChanges.ElapsedMilliseconds}ms"
            });
        }

        [HttpPost("InsertBenchMarkT")]
        public ActionResult<BenchmarkResult> InsertBenchMarkT()
        {
            var customers = GenerateCustomers(50000);
            var clockSaveChanges = new Stopwatch();

            //normal insert
            _dbContext.BulkInsert(customers); //from nuget package: Z.EntityFramework.Extensions.EFCore
            clockSaveChanges.Start();
            _dbContext.SaveChanges();
            clockSaveChanges.Stop();

            return Ok(new BenchmarkResult
            {
                Action = "EF Core SaveChanges",
                Entities = customers.Count,
                TimeElapsed = $"{clockSaveChanges.ElapsedMilliseconds}ms"
            });
        }

        [HttpPost("InsertWithOptimizationFlag")]
        public ActionResult<BenchmarkResult> InsertWithOptimizationFlag()
        {
            var customers = GenerateCustomers(50000);
            var clockSaveChanges = new Stopwatch();

            //normal insert
            _dbContext.BulkInsert(customers, options => options.AutoMapOutputDirection = false); //from nuget package: Z.EntityFramework.Extensions.EFCore
            clockSaveChanges.Start();
            _dbContext.SaveChanges();
            clockSaveChanges.Stop();

            return Ok(new BenchmarkResult
            {
                Action = "EF Core SaveChanges",
                Entities = customers.Count,
                TimeElapsed = $"{clockSaveChanges.ElapsedMilliseconds}ms"
            });
        }

        [HttpPut("UpdateNormal")]
        public ActionResult<BenchmarkResult> UpdateNormal()
        {
            var customers = _dbContext.Customers.ToList();
            var clockSaveChanges = new Stopwatch();

            //normal insert
            foreach (var customer in customers)
            {
                customer.Name = "Updated_" + customer.Name;
                customer.Description += "_Updated";
            };

            clockSaveChanges.Start();
            _dbContext.SaveChanges();
            clockSaveChanges.Stop();

            return Ok(new BenchmarkResult
            {
                Action = "EF Core SaveChanges",
                Entities = customers.Count,
                TimeElapsed = $"{clockSaveChanges.ElapsedMilliseconds}ms"
            });
        }

        [HttpPut("UpdateBenchmark")]
        public ActionResult<BenchmarkResult> UpdateBenchmark()
        {
            var customers = _dbContext.Customers.ToList();
            var clockSaveChanges = new Stopwatch();

            //normal insert
            foreach (var customer in customers)
            {
                customer.Name = "Updated_" + customer.Name;
                customer.Description += "_Updated";
            };

            clockSaveChanges.Start();
            _dbContext.BulkUpdate(customers);
            clockSaveChanges.Stop();

            return Ok(new BenchmarkResult
            {
                Action = "EF Core SaveChanges",
                Entities = customers.Count,
                TimeElapsed = $"{clockSaveChanges.ElapsedMilliseconds}ms"
            });
        }



        private static List<Customer> GenerateCustomers(int count)
        {
            var list = new List<Customer>();

            for (int i = 0; i < count; i++)
            {
                list.Add(new Customer() { Name = "Customer_" + i, Description = "Description_" + i, IsActive = i % 2 == 0 });
            }

            return list;
        }
    }
}
