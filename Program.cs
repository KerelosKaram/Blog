using Blog.Configuration;
using Blog.Data;
using Blog.Data.FileManager;
using Blog.Data.Repository;
using Blog.Services.Email;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using static System.Net.Mime.MediaTypeNames;

namespace Blog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<SmtpSettings>(options => {
                options.From = "test@raw-blog.net"; //error ???
                options.Server = "mail.raw-coding.net";
                options.Username = "test@raw-coding.net";
                options.Password = "!234QWERasdf";
            });

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });


            builder.Services.AddTransient<IRepository, Repository>();
            builder.Services.AddTransient<IFileManager, FileManager>();
            builder.Services.AddSingleton<IEmailService, EmailService>();

            builder.Services.AddMvc(options =>
            {
                options.CacheProfiles.Add("Monthly", new CacheProfile { Duration = 60 * 60 * 24 * 7 * 4 });
            });


            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
              //.AddRoles<IdentityRole>()
              .AddEntityFrameworkStores<AppDbContext>();


            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Auth/Login";
            });


            var app = builder.Build();

            app.UseDeveloperExceptionPage(); // Enable it for Publishing

            try
            {
                var scope = app.Services.CreateScope();
                var ctx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                ctx.Database.EnsureCreated();

                var adminRole = new IdentityRole("admin");

                if (!ctx.Roles.Any())
                {
                    // create a role
                    roleMgr.CreateAsync(adminRole).GetAwaiter().GetResult();
                }

                if (!ctx.Users.Any(u => u.UserName == "admin"))
                {
                    // create an admin
                    var adminUser = new IdentityUser
                    {
                        UserName = "admin",
                        Email = "admin@test.com"
                    };
                    var result = userMgr.CreateAsync(adminUser, "password").GetAwaiter().GetResult();
                    // add role to user
                    userMgr.AddToRoleAsync(adminUser, adminRole.Name).GetAwaiter().GetResult();
                }

                //if(!ctx.Posts.Any(p => p.Id == 1))
                //{
                //    var defaultPost = new Post
                //    {
                //        Id = 1,
                //        Title = "Test",
                //        Body = "testtesttest",
                //        Created = DateTime.Now
                //    };
                //    ctx.SaveChangesAsync().GetAwaiter().GetResult();
                //}

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }



            app.UseRouting();

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

#pragma warning disable ASP0014 // Suggest using top level route registrations
            app.UseEndpoints(endpoints =>
            {

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}"
                );

            });
#pragma warning restore ASP0014 // Suggest using top level route registrations







            app.Run();
        }
    }
}