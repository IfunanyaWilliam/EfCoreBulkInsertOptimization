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
        }

        [HttpGet("ReadNormal")]
        public async Task<ActionResult<BenchmarkResult>> NormalRead()
        {
            var clockRead = new Stopwatch();
            clockRead.Start();
            var customers = await _dbContext.Customers.ToListAsync();
            clockRead.Stop();

            return Ok(new BenchmarkResult
            {
                Action = "EF Core Read",
                Entities = customers.Count,
                TimeElapsed = $"{clockRead.ElapsedMilliseconds}ms"
            });
        }

        //I didn't get any performance improvement with this method, but it's here for comparison
        //Check this method for possible improvements
        [HttpGet("ReadOptimized")]
        public async Task<ActionResult<BenchmarkResult>> OptimizedRead()
        {
            var clockRead = new Stopwatch();
            var customerIds = Enumerable.Range(1, 50000).ToList();

            clockRead.Start();
            var customers = await _dbContext.Customers.AsNoTracking()
                        .WhereBulkContains(customerIds, c => c.Id).ToListAsync(); 
            clockRead.Stop();

            return Ok(new BenchmarkResult
            {
                Action = "EF Core Read Optimized",
                Entities = customers.Count,
                TimeElapsed = $"{clockRead.ElapsedMilliseconds}ms"
            });
        }

        [HttpPost("InsertNormal")]
        public ActionResult<BenchmarkResult> InsertNormal()
        {
            var customers = GenerateCustomers(5000);
            var clockSaveChanges = new Stopwatch();

            //normal insert
            clockSaveChanges.Start();
            _dbContext.Customers.AddRange(customers);
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
            clockSaveChanges.Start();
            _dbContext.BulkInsert(customers); //from nuget package: Z.EntityFramework.Extensions.EFCore
            //_dbContext.SaveChanges();
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
            clockSaveChanges.Start();
            _dbContext.BulkInsert(customers, options => options.AutoMapOutputDirection = false); //from nuget package: Z.EntityFramework.Extensions.EFCore
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
