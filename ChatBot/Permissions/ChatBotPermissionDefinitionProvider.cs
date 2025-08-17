using ChatBot.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace ChatBot.Permissions;

public class ChatBotPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(ChatBotPermissions.GroupName);



        //Define your own permissions here. Example:
        //myGroup.AddPermission(ChatBotPermissions.MyPermission1, L("Permission:MyPermission1"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<ChatBotResource>(name);
    }
}
