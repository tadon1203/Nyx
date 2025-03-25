namespace Nyx.Core.Configuration;

public interface IConfigurableModule
{
	void SaveModuleConfig(ModuleConfig config);
	void LoadModuleConfig(ModuleConfig config);
}