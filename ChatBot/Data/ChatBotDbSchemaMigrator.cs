﻿using Volo.Abp.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace ChatBot.Data;

public class ChatBotDbSchemaMigrator : ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public ChatBotDbSchemaMigrator(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        
        /* We intentionally resolving the ChatBotDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<ChatBotDbContext>()
            .Database
            .MigrateAsync();

    }
}
