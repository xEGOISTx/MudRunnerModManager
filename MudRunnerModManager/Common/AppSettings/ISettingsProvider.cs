using System.Threading.Tasks;

namespace MudRunnerModManager.Common.AppSettings
{
    public interface ISettingsProvider
    {
        Task LoadAsync(ISettings settings);
        Task SaveAsync(ISettings settings);
    }
}
