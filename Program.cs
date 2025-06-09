namespace WawaEditor;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        
        try
        {
            // 确保配置已加载，并在启动前预加载配置
            var config = AppConfig.Instance;
            
            // 确保配置已保存，这样即使是首次运行也会创建配置文件
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wawaeditor_config.json");
            if (!File.Exists(configPath))
            {
                config.Save();
            }
            else
            {
                // 验证配置文件是否有效
                try
                {
                    string json = File.ReadAllText(configPath);
                    if (string.IsNullOrEmpty(json))
                    {
                        // 配置文件为空，重新创建
                        config.Save();
                    }
                }
                catch
                {
                    // 配置文件读取失败，重新创建
                    config.Save();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"配置初始化失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        Application.Run(new MainForm());
    }    
}