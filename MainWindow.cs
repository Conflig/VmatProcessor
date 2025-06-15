using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Documents;

namespace VmatProcessor
{
    public class MainWindow : Window
    {
        private List<string> vmatFiles = new List<string>();
        private List<string> validPngPaths = new List<string>();
        private List<string> finalVmatList = new List<string>();
        private string selectedDirectory = "";

        // Color palette based on the provided image
        private static readonly Color PrimaryColor = Color.FromRgb(255, 45, 85);      // Pink/Red
        private static readonly Color SecondaryColor = Color.FromRgb(142, 69, 133);   // Purple
        private static readonly Color TertiaryColor = Color.FromRgb(88, 86, 214);     // Blue-Purple
        private static readonly Color DarkColor = Color.FromRgb(25, 25, 112);         // Dark Blue
        private static readonly Color BackgroundColor = Color.FromRgb(30, 30, 35);    // Dark Gray
        private static readonly Color SurfaceColor = Color.FromRgb(40, 40, 45);       // Lighter Dark Gray
        private static readonly Color TextColor = Color.FromRgb(240, 240, 240);       // Light Gray
        private static readonly Color AccentColor = Color.FromRgb(100, 100, 105);     // Medium Gray

        // UI Controls
        private Label directoryLabel;
        private Button processButton;
        private ProgressBar progressBar;
        private Label statusLabel;
        private Label vmatCountLabel;
        private Label pngCountLabel;
        private Label finalCountLabel;
        private Button openPngButton;
        private Button openVmatButton;
        private Button openJpgButton;  // New button for JPG list
        private TextBox blenderPathTextBox;  // New text box for Blender addon path
        private Button installBlenderPluginButton;  // New button for installing Blender plugin
        private TextBox logTextBox;

        public MainWindow()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            // Set window properties
            Title = "VMAT File Processor";
            Width = 900;
            Height = 700;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(BackgroundColor);
            
            // Create main grid
            var mainGrid = new Grid();
            mainGrid.Margin = new Thickness(20);
            
            // Define rows
            for (int i = 0; i < 13; i++)  // Increased from 10 to 13 for new elements
            {
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Title
            var titleLabel = new Label
            {
                Content = "VMAT File Processor",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = new SolidColorBrush(PrimaryColor)
            };
            Grid.SetRow(titleLabel, 0);
            mainGrid.Children.Add(titleLabel);

            // Directory selection
            var dirPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 20, 0, 10) };
            
            var selectDirButton = new Button
            {
                Content = "Select Directory",
                Width = 120,
                Height = 30,
                Margin = new Thickness(0, 0, 10, 0)
            };
            selectDirButton.Style = CreateCustomButtonStyle();
            selectDirButton.Click += SelectDirectory_Click;
            
            directoryLabel = new Label
            {
                Content = "No directory selected",
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(AccentColor)
            };
            
            dirPanel.Children.Add(selectDirButton);
            dirPanel.Children.Add(directoryLabel);
            Grid.SetRow(dirPanel, 1);
            mainGrid.Children.Add(dirPanel);

            // Process button
            processButton = new Button
            {
                Content = "Start Processing",
                Width = 150,
                Height = 40,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                IsEnabled = false,
                Margin = new Thickness(0, 10, 0, 20)
            };
            
            // Apply custom style to override default button styling
            processButton.Style = CreateCustomButtonStyle();
            processButton.Click += ProcessFiles_Click;
            Grid.SetRow(processButton, 2);
            mainGrid.Children.Add(processButton);

            // Progress bar
            progressBar = new ProgressBar
            {
                Height = 20,
                Margin = new Thickness(0, 0, 0, 10),
                Visibility = Visibility.Collapsed,
                Background = new SolidColorBrush(SurfaceColor),
                Foreground = new SolidColorBrush(PrimaryColor)
            };
            Grid.SetRow(progressBar, 3);
            mainGrid.Children.Add(progressBar);

            // Status label
            statusLabel = new Label
            {
                Content = "Ready to process files",
                Margin = new Thickness(0, 0, 0, 10),
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(TextColor)
            };
            Grid.SetRow(statusLabel, 4);
            mainGrid.Children.Add(statusLabel);

            // Results section
            var resultsLabel = new Label
            {
                Content = "Results:",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 20, 0, 10),
                Foreground = new SolidColorBrush(SecondaryColor)
            };
            Grid.SetRow(resultsLabel, 5);
            mainGrid.Children.Add(resultsLabel);

            // Statistics
            var statsPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
            
            vmatCountLabel = new Label { Content = "VMAT files: 0", Margin = new Thickness(0, 0, 20, 0), Foreground = new SolidColorBrush(TextColor) };
            pngCountLabel = new Label { Content = "Valid PNG files: 0", Margin = new Thickness(0, 0, 20, 0), Foreground = new SolidColorBrush(TextColor) };
            finalCountLabel = new Label { Content = "Final materials: 0", Foreground = new SolidColorBrush(TextColor) };
            
            statsPanel.Children.Add(vmatCountLabel);
            statsPanel.Children.Add(pngCountLabel);
            statsPanel.Children.Add(finalCountLabel);
            Grid.SetRow(statsPanel, 6);
            mainGrid.Children.Add(statsPanel);

            // Output buttons
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 10) };
            
            openPngButton = new Button
            {
                Content = "Open PNG List",
                Width = 120,
                Height = 30,
                Margin = new Thickness(0, 0, 10, 0),
                IsEnabled = false
            };
            openPngButton.Style = CreateCustomButtonStyle();
            openPngButton.Click += OpenPngList_Click;
            
            // New JPG button
            openJpgButton = new Button
            {
                Content = "Open JPG List",
                Width = 120,
                Height = 30,
                Margin = new Thickness(0, 0, 10, 0),
                IsEnabled = false
            };
            openJpgButton.Style = CreateCustomButtonStyle();
            openJpgButton.Click += OpenJpgList_Click;
            
            openVmatButton = new Button
            {
                Content = "Open VMAT List",
                Width = 120,
                Height = 30,
                IsEnabled = false
            };
            openVmatButton.Style = CreateCustomButtonStyle();
            openVmatButton.Click += OpenVmatList_Click;
            
            buttonPanel.Children.Add(openPngButton);
            buttonPanel.Children.Add(openJpgButton);
            buttonPanel.Children.Add(openVmatButton);
            Grid.SetRow(buttonPanel, 7);
            mainGrid.Children.Add(buttonPanel);

            // Blender Plugin Installation Section
            var blenderSectionLabel = new Label
            {
                Content = "Blender Plugin Installation:",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 20, 0, 5),
                Foreground = new SolidColorBrush(SecondaryColor)
            };
            Grid.SetRow(blenderSectionLabel, 8);
            mainGrid.Children.Add(blenderSectionLabel);

            // Hint label for Blender path
            var blenderHintLabel = new Label
            {
                Content = "Blender addons folder (example: C:\\Users\\User\\AppData\\Roaming\\Blender Foundation\\Blender\\4.4\\scripts\\addons)",
                FontSize = 10,
                FontStyle = FontStyles.Italic,
                Margin = new Thickness(0, 0, 0, 5),
                Foreground = new SolidColorBrush(AccentColor)
            };
            Grid.SetRow(blenderHintLabel, 9);
            mainGrid.Children.Add(blenderHintLabel);

            // Blender path input and install button
            var blenderPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
            
            blenderPathTextBox = new TextBox
            {
                Width = 500,
                Height = 25,
                Margin = new Thickness(0, 0, 10, 0),
                Background = new SolidColorBrush(SurfaceColor),
                Foreground = new SolidColorBrush(TextColor),
                BorderBrush = new SolidColorBrush(AccentColor),
                BorderThickness = new Thickness(1),
                VerticalContentAlignment = VerticalAlignment.Center
            };
            
            var browseBlenderButton = new Button
            {
                Content = "Browse",
                Width = 70,
                Height = 25,
                Margin = new Thickness(0, 0, 10, 0)
            };
            browseBlenderButton.Style = CreateCustomButtonStyle();
            browseBlenderButton.Click += BrowseBlenderPath_Click;
            
            installBlenderPluginButton = new Button
            {
                Content = "Install Blender Plugin",
                Width = 150,
                Height = 25
            };
            installBlenderPluginButton.Style = CreateCustomButtonStyle();
            installBlenderPluginButton.Click += InstallBlenderPlugin_Click;
            
            blenderPanel.Children.Add(blenderPathTextBox);
            blenderPanel.Children.Add(browseBlenderButton);
            blenderPanel.Children.Add(installBlenderPluginButton);
            Grid.SetRow(blenderPanel, 10);
            mainGrid.Children.Add(blenderPanel);

            // Log text box
            logTextBox = new TextBox
            {
                Margin = new Thickness(0, 10, 0, 0),
                IsReadOnly = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 11,
                Background = new SolidColorBrush(SurfaceColor),
                Foreground = new SolidColorBrush(TextColor),
                BorderBrush = new SolidColorBrush(AccentColor),
                BorderThickness = new Thickness(1)
            };
            Grid.SetRow(logTextBox, 11);
            mainGrid.Children.Add(logTextBox);

            Content = mainGrid;
        }

        private Style CreateCustomButtonStyle()
        {
            var style = new Style(typeof(Button));
            
            // Set normal appearance
            style.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(SurfaceColor)));
            style.Setters.Add(new Setter(Button.ForegroundProperty, new SolidColorBrush(TextColor)));
            style.Setters.Add(new Setter(Button.BorderBrushProperty, new SolidColorBrush(TertiaryColor)));
            style.Setters.Add(new Setter(Button.BorderThicknessProperty, new Thickness(2)));
            
            // Create a simpler control template
            var template = new ControlTemplate(typeof(Button));
            
            // Create the border element
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Button.BackgroundProperty));
            borderFactory.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Button.BorderBrushProperty));
            borderFactory.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Button.BorderThicknessProperty));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
            
            // Create the content presenter
            var contentFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            contentFactory.SetValue(TextElement.ForegroundProperty, new TemplateBindingExtension(Button.ForegroundProperty));
            
            borderFactory.AppendChild(contentFactory);
            template.VisualTree = borderFactory;
            
            // Set triggers for different states
            var enabledTrigger = new Trigger { Property = Button.IsEnabledProperty, Value = false };
            enabledTrigger.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(AccentColor)));
            enabledTrigger.Setters.Add(new Setter(Button.ForegroundProperty, new SolidColorBrush(Color.FromRgb(180, 180, 180))));
            template.Triggers.Add(enabledTrigger);
            
            var mouseOverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
            mouseOverTrigger.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(TertiaryColor)));
            template.Triggers.Add(mouseOverTrigger);
            
            style.Setters.Add(new Setter(Button.TemplateProperty, template));
            
            return style;
        }

        private void SelectDirectory_Click(object sender, RoutedEventArgs e)
        {
            // Use a simple input dialog approach
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Select Directory (any filename, we'll use the folder)",
                FileName = "select_this_folder",
                DefaultExt = ".txt",
                Filter = "Text files (*.txt)|*.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                selectedDirectory = System.IO.Path.GetDirectoryName(dialog.FileName);
                directoryLabel.Content = selectedDirectory;
                directoryLabel.Foreground = new SolidColorBrush(TextColor);
                processButton.IsEnabled = true;
                LogMessage($"Selected directory: {selectedDirectory}");
            }
        }

        private async void ProcessFiles_Click(object sender, RoutedEventArgs e)
        {
            processButton.IsEnabled = false;
            progressBar.Visibility = Visibility.Visible;
            progressBar.Value = 0;

            try
            {
                await ProcessFilesAsync();
            }
            catch (Exception ex)
            {
                LogMessage($"Error: {ex.Message}");
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                processButton.IsEnabled = true;
                progressBar.Visibility = Visibility.Collapsed;
            }
        }

        private async Task ProcessFilesAsync()
        {
            // Step 1: Find VMAT files
            statusLabel.Content = "Step 1: Finding VMAT files...";
            LogMessage("=== Step 1: Finding VMAT files ===");
            
            vmatFiles.Clear();
            await Task.Run(() =>
            {
                var files = Directory.GetFiles(selectedDirectory, "*.vmat", SearchOption.AllDirectories);
                vmatFiles.AddRange(files);
            });

            LogMessage($"Found {vmatFiles.Count} VMAT files");
            vmatCountLabel.Content = $"VMAT files: {vmatFiles.Count}";
            progressBar.Value = 20;

            if (vmatFiles.Count == 0)
            {
                LogMessage("No VMAT files found!");
                statusLabel.Content = "No VMAT files found";
                return;
            }

            // Step 2: Convert to PNG paths
            statusLabel.Content = "Step 2: Converting to PNG paths...";
            LogMessage("=== Step 2: Converting to PNG paths ===");
            
            var pngPaths = new List<string>();
            await Task.Run(() =>
            {
                foreach (var vmatFile in vmatFiles)
                {
                    var pngPath = vmatFile.Replace(".vmat", "_color.png");
                    pngPaths.Add(pngPath);
                }
            });

            progressBar.Value = 40;

            // Step 3: Validate PNG files
            statusLabel.Content = "Step 3: Validating PNG files...";
            LogMessage("=== Step 3: Validating PNG files ===");
            
            validPngPaths.Clear();
            await Task.Run(() =>
            {
                foreach (var pngPath in pngPaths)
                {
                    if (File.Exists(pngPath))
                    {
                        validPngPaths.Add(pngPath);
                    }
                }
            });

            LogMessage($"Found {validPngPaths.Count} valid PNG files out of {pngPaths.Count}");
            pngCountLabel.Content = $"Valid PNG files: {validPngPaths.Count}";
            progressBar.Value = 60;

            // Step 4: Create final VMAT list
            statusLabel.Content = "Step 4: Creating final VMAT list...";
            LogMessage("=== Step 4: Creating final VMAT list ===");
            
            finalVmatList.Clear();
            await Task.Run(() =>
            {
                foreach (var pngPath in validPngPaths)
                {
                    var vmatPath = pngPath.Replace("_color.png", ".vmat");
                    var materialsIndex = vmatPath.ToLower().IndexOf("\\materials\\");
                    
                    if (materialsIndex >= 0)
                    {
                        var relativePath = vmatPath.Substring(materialsIndex + 1); // +1 to remove leading backslash
                        finalVmatList.Add(relativePath);
                    }
                }
            });

            finalCountLabel.Content = $"Final materials: {finalVmatList.Count}";
            progressBar.Value = 80;

            // Save files
            statusLabel.Content = "Saving output files...";
            LogMessage("=== Saving output files ===");
            
            await SaveOutputFiles();

            progressBar.Value = 100;

            statusLabel.Content = $"Processing complete! Found {finalVmatList.Count} materials.";
            LogMessage($"Processing complete! Created {finalVmatList.Count} material entries.");
            
            openPngButton.IsEnabled = validPngPaths.Count > 0;
            openJpgButton.IsEnabled = validPngPaths.Count > 0;  // Enable JPG button when PNG files exist
            openVmatButton.IsEnabled = finalVmatList.Count > 0;
        }

        private async Task SaveOutputFiles()
        {
            var pngListPath = Path.Combine(selectedDirectory, "valid_png_paths.txt");
            var jpgListPath = Path.Combine(selectedDirectory, "valid_jpg_paths.txt");
            var vmatListPath = Path.Combine(selectedDirectory, "final_vmat_list.txt");

            await Task.Run(() =>
            {
                // Save PNG list
                File.WriteAllLines(pngListPath, validPngPaths);
                
                // Create and save JPG list (convert .png to .jpg)
                var jpgPaths = validPngPaths.Select(path => path.Replace(".png", ".jpg")).ToList();
                File.WriteAllLines(jpgListPath, jpgPaths);
                
                // Save VMAT list
                File.WriteAllLines(vmatListPath, finalVmatList);
            });

            LogMessage($"Saved: {pngListPath}");
            LogMessage($"Saved: {jpgListPath}");
            LogMessage($"Saved: {vmatListPath}");
        }

        private void OpenPngList_Click(object sender, RoutedEventArgs e)
        {
            var filePath = Path.Combine(selectedDirectory, "valid_png_paths.txt");
            if (File.Exists(filePath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
            }
        }

        private void OpenJpgList_Click(object sender, RoutedEventArgs e)
        {
            var filePath = Path.Combine(selectedDirectory, "valid_jpg_paths.txt");
            if (File.Exists(filePath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
            }
        }

        private void OpenVmatList_Click(object sender, RoutedEventArgs e)
        {
            var filePath = Path.Combine(selectedDirectory, "final_vmat_list.txt");
            if (File.Exists(filePath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
            }
        }

        private void LogMessage(string message)
        {
            Dispatcher.Invoke(() =>
            {
                logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                logTextBox.ScrollToEnd();
            });
        }

        private void BrowseBlenderPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Select Blender Addons Folder",
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Select this folder",
                Filter = "Folders|*.folder"
            };

            // Use SaveFileDialog to select folder
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Select Blender Addons Folder",
                FileName = "select_addons_folder",
                DefaultExt = ".txt",
                Filter = "Text files (*.txt)|*.txt"
            };

            if (saveDialog.ShowDialog() == true)
            {
                var selectedPath = System.IO.Path.GetDirectoryName(saveDialog.FileName);
                blenderPathTextBox.Text = selectedPath;
                LogMessage($"Selected Blender addons path: {selectedPath}");
            }
        }

        private void InstallBlenderPlugin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var blenderAddonsPath = blenderPathTextBox.Text.Trim();
                
                if (string.IsNullOrEmpty(blenderAddonsPath))
                {
                    MessageBox.Show("Please specify the Blender addons folder path.", "Path Required", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!Directory.Exists(blenderAddonsPath))
                {
                    MessageBox.Show("The specified Blender addons folder does not exist.", "Invalid Path", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Look for the Python file in the current directory
                var currentDir = AppDomain.CurrentDomain.BaseDirectory;
                var pythonFile = Path.Combine(currentDir, "blender_material_creator.py");

                if (!File.Exists(pythonFile))
                {
                    MessageBox.Show("blender_material_creator.py not found in the application directory.", "File Not Found", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    LogMessage($"Error: blender_material_creator.py not found at {pythonFile}");
                    return;
                }

                // Copy the file to Blender addons folder
                var destinationPath = Path.Combine(blenderAddonsPath, "blender_material_creator.py");
                
                File.Copy(pythonFile, destinationPath, overwrite: true);

                LogMessage($"Successfully installed Blender plugin to: {destinationPath}");
                MessageBox.Show($"Blender plugin successfully installed!\n\nLocation: {destinationPath}\n\nYou can now enable it in Blender's addon preferences.", 
                    "Installation Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Access denied. Please run the application as administrator or choose a different folder.", 
                    "Permission Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LogMessage("Error: Access denied when copying Blender plugin");
            }
            catch (Exception ex)
            {
                LogMessage($"Error installing Blender plugin: {ex.Message}");
                MessageBox.Show($"Error installing Blender plugin: {ex.Message}", "Installation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class App : Application
    {
        [STAThread]
        public static void Main()
        {
            var app = new App();
            app.Run(new MainWindow());
        }
    }
}