using Microsoft.Extensions.AI;

namespace Chat.Infrastructure.Tools;

public interface IChatTool
{
    AIFunction Create();
}
