using Microsoft.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

using ChatBot.Entities; //  OpenAiThreadCacheDetails i entities den okuması icin

namespace ChatBot.Data;

public class ChatBotDbContext : AbpDbContext<ChatBotDbContext>
{

    public const string DbTablePrefix = "App";
    public const string DbSchema = null;

    // OpenAiThreadCacheDetails classinin tablosunu olusturuyor Db de
    public DbSet<ChatMessageContent> ChatMessageContent { get; set; }


    public ChatBotDbContext(DbContextOptions<ChatBotDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);    // BU SATIR TÜM ABP MODÜLLERİNİ EKLER

        /* Include modules to your migration db context */

        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigurePermissionManagement();
        builder.ConfigureBlobStoring();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();

        // OpenAI Chat geçmişi tablosunu yapılandır

        builder.Entity<ChatMessageContent>(b =>
        {
            b.ToTable(ChatBotDbContext.DbTablePrefix + "ChatMessage", ChatBotDbContext.DbSchema);

            b.HasKey(x => x.Id);

            b.Property(x => x.SessionId);
            b.Property(x => x.Role).IsRequired().HasMaxLength(32);
            b.Property(x => x.Message).IsRequired().HasMaxLength(4000);
            b.Property(x => x.Response).HasMaxLength(4000);
            b.Property(x => x.Timestamp).IsRequired();
        });


        /* Configure your own entities here */
    }
}

