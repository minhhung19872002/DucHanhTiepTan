using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DucHanhTiepTan.Api.Data;

/// <summary>
/// Applies migrations, (re)creates SQL views and loads the seed data on first run.
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, ILogger logger)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Wait for PostgreSQL (docker compose may start the API before the DB is ready).
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync();
                break;
            }
            catch (Exception ex) when (ex is NpgsqlException or TimeoutException or System.Net.Sockets.SocketException)
            {
                if (attempt >= 30) throw;
                logger.LogWarning("Database not ready ({Message}); retrying in 2s ({Attempt}/30)", ex.Message, attempt);
                await Task.Delay(2000);
            }
        }

        var viewsPath = Path.Combine(AppContext.BaseDirectory, "Sql", "views.sql");
        if (File.Exists(viewsPath))
        {
            await ExecuteScriptAsync(db, await File.ReadAllTextAsync(viewsPath));
            logger.LogInformation("SQL views ensured");
        }

        if (!await db.Branches.AnyAsync())
        {
            var seedDir = config["Seed:Directory"];
            if (string.IsNullOrWhiteSpace(seedDir))
                seedDir = Path.Combine(env.ContentRootPath, "..", "..", "db", "seed");
            if (Directory.Exists(seedDir))
            {
                foreach (var file in Directory.GetFiles(seedDir, "*.sql").OrderBy(f => f))
                {
                    logger.LogInformation("Loading seed file {File}", Path.GetFileName(file));
                    await ExecuteScriptAsync(db, await File.ReadAllTextAsync(file));
                }
                await ResetSequencesAsync(db);
                logger.LogInformation("Seed data loaded");
            }
            else
            {
                logger.LogWarning("Seed directory {Dir} not found; starting with an empty database", seedDir);
            }
        }

        // Guarantee at least one administrator account exists.
        if (!await db.Accounts.AnyAsync(a => a.IsAdministrator))
        {
            if (!await db.Branches.AnyAsync())
            {
                db.Branches.Add(new Entities.Branch { Id = 0, Name = "Tất cả" });
                db.Branches.Add(new Entities.Branch { Id = 1, Name = "Chi nhánh 1" });
                await db.SaveChangesAsync();
            }
            var branchId = await db.Branches.Where(b => b.Id != 0).Select(b => b.Id).FirstAsync();
            db.Accounts.Add(new Entities.Account
            {
                UserName = "admin",
                FullName = "Administrator",
                IsAdministrator = true,
                BranchId = branchId,
                Password = Security.Md5Hasher.Hash("admin"),
            });
            await db.SaveChangesAsync();
            logger.LogInformation("Default admin account created (admin/admin)");
        }
    }

    /// <summary>Runs a raw SQL script without EF parameter/format processing (scripts may contain braces).</summary>
    private static async Task ExecuteScriptAsync(AppDbContext db, string sql)
    {
        var conn = db.Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandTimeout = 600;
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>Align identity sequences with explicitly inserted ids.</summary>
    private static async Task ResetSequencesAsync(AppDbContext db)
    {
        const string sql = """
            DO $$
            DECLARE r RECORD;
            BEGIN
              FOR r IN
                SELECT c.relname AS tbl, a.attname AS col, pg_get_serial_sequence(quote_ident(c.relname), a.attname) AS seq
                FROM pg_class c
                JOIN pg_attribute a ON a.attrelid = c.oid
                JOIN pg_namespace n ON n.oid = c.relnamespace
                WHERE n.nspname = 'public' AND c.relkind = 'r' AND a.attidentity <> ''
              LOOP
                EXECUTE format('SELECT setval(%L, COALESCE((SELECT MAX(%I) FROM %I), 0) + 1, false)', r.seq, r.col, r.tbl);
              END LOOP;
            END $$;
            """;
        await ExecuteScriptAsync(db, sql);
    }
}
