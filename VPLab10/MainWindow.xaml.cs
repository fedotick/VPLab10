using LiveCharts.Wpf;
using LiveCharts;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.ConstrainedExecution;

namespace VPLab10
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string currentDirectory = Directory.GetCurrentDirectory();
        private Random random = new Random();

        private string category = "all";
        private List<string> words;

        private DispatcherTimer timer;
        private int totalTime = 60; 
        private int elapsedTime = 0;

        private int numberOfAnswers = 0;
        private int numberOfCorrectAnswers = 0;

        private Dictionary<string, List<Dictionary<string, int>>> users;

        public MainWindow()
        {
            InitializeComponent();
            InitializeWords();
            InitializeTimer();
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            gridMenu.Visibility = Visibility.Hidden;
            
            UpdateProgressBar();
            UpdateTimeRemaining();
            NextImage();

            gridMain.Visibility = Visibility.Visible;

            timer.Start();
        }

        private void ButtonRules_Click(object sender, RoutedEventArgs e)
        {
            gridMenu.Visibility = Visibility.Hidden;
            gridRules.Visibility = Visibility.Visible;
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            gridRules.Visibility = Visibility.Hidden;
            gridMenu.Visibility = Visibility.Visible;
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            gridMenu.Visibility = Visibility.Hidden;
            gridSettings.Visibility = Visibility.Visible;

            comboBoxCategory.Text = category;
            textBoxTime.Text = totalTime.ToString();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            gridSettings.Visibility = Visibility.Hidden;
            
            category = comboBoxCategory.Text;
            InitializeWords();
            totalTime = Convert.ToInt16(textBoxTime.Text);

            gridMenu.Visibility = Visibility.Visible;
        }

        private void ButtonStatistics_Click(object sender, RoutedEventArgs e)
        {
            gridMenu.Visibility = Visibility.Hidden;

            ReadStatistics();
            CreateChart();

            gridChart.Visibility = Visibility.Visible;
        }

        private void CreateChart()
        {
            // List<string> userNames = new List<string>();
            SeriesCollection = new SeriesCollection();

            foreach (var user in users)
            {
                string userName = user.Key;
                // userNames.Add(userName);

                List<Dictionary<string, int>> userStatistics = user.Value;

                foreach (var userStatisticsItems in userStatistics)
                {
                    double responsesPerSecond = Math.Round(Convert.ToDouble(userStatisticsItems["correctAnswers"]) / userStatisticsItems["time"], 2);

                    SeriesCollection.Add(new ColumnSeries
                    {
                        Title = userName,
                        Values = new ChartValues<double> { responsesPerSecond }
                    });
                }
            }

            // Labels = userNames.ToArray();
            Labels = new[] { "" };
            Formatter = value => value.ToString("N");

            DataContext = this;
        }

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }

        private void ReadStatistics()
        {
            users = new Dictionary<string, List<Dictionary<string, int>>>();

            var lines = File.ReadAllLines(currentDirectory + "/statistics.txt");

            foreach (var line in lines)
            {
                var values = line.Split('|');

                string userName = values[0];
                int correctAnswers = int.Parse(values[1]);
                int wrongAnswers = int.Parse(values[2]);
                int time = int.Parse(values[3]);

                if (!users.ContainsKey(userName))
                {
                    users[userName] = new List<Dictionary<string, int>>();
                }

                Dictionary<string, int> statisticsItem = new Dictionary<string, int>
                {
                    { "correctAnswers", correctAnswers },
                    { "wrongAnswers", wrongAnswers },
                    { "time", time },
                };

                users[userName].Add(statisticsItem);
            }
        }

        private void ButtonBack_Click(object sender, EventArgs e)
        {
            gridChart.Visibility = Visibility.Hidden;
            gridMenu.Visibility = Visibility.Visible;
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            gridMenu.Visibility = Visibility.Visible;
            gridMain.Visibility = Visibility.Hidden;

            timer.Stop();

            elapsedTime = 0;
            numberOfAnswers = 0;
            numberOfCorrectAnswers = 0;
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            NextImage();
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            gridScore.Visibility = Visibility.Hidden;

            SaveStatistics();

            numberOfAnswers = 0;
            numberOfCorrectAnswers = 0;

            gridMenu.Visibility = Visibility.Visible;
        }

        private void SaveStatistics()
        {
            string userName = textBoxUserName.Text;

            if (userName == "")
            {
                userName = "guest";
            }

            using (StreamWriter writer = new StreamWriter(currentDirectory + "/statistics.txt", true))
            {
                writer.WriteLine($"{userName}|{numberOfCorrectAnswers}|{numberOfAnswers - numberOfCorrectAnswers}|{totalTime}");
            }
        }

        private void TextBoxAnswer_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (imagePreview.Source is BitmapImage bitmapImage)
            {
                string absolutePath = bitmapImage.UriSource.AbsolutePath;
                string fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(absolutePath);

                if (textBoxAnswer.Text == fileNameWithoutExtension)
                {
                    numberOfCorrectAnswers++;
                    NextImage();
                }
            }
        }
        
        private void NextImage()
        {
            numberOfAnswers++;
            imagePreview.Source = new BitmapImage(new Uri($"{currentDirectory}/img/{words[random.Next(words.Count)]}"));
            textBoxAnswer.Text = "";
        }

        private void InitializeWords()
        {
            words = new List<string>();

            if (category == "all")
            {
                string[] directoryPaths = Directory.GetDirectories(currentDirectory + @"\img");

                foreach (string directoryPath in directoryPaths)
                {
                    words.AddRange(GetFiles(directoryPath));
                }
            }
            else
            {
                words.AddRange(GetFiles($"{currentDirectory}/img/{category}"));
            }
        }

        private List<string> GetFiles(string directoryPath)
        {
            string directory = new DirectoryInfo(directoryPath).Name;
            string[] filePaths = Directory.GetFiles(directoryPath);

            for (int i = 0; i < filePaths.Length; i++)
            {
                filePaths[i] = $"{directory}/{System.IO.Path.GetFileName(filePaths[i])}";
            }

            return filePaths.ToList();
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            elapsedTime++;
            UpdateProgressBar();
            UpdateTimeRemaining();

            if (elapsedTime >= totalTime)
            {
                timer.Stop();
                elapsedTime = 0;
                numberOfAnswers--;
                ShowScore();
            }
        }

        private void UpdateProgressBar()
        {
            double progress = (double)elapsedTime / totalTime * 100;
            progressBar.Value = progress;
        }

        private void UpdateTimeRemaining()
        {
            int remainingTime = totalTime - elapsedTime;
            textBlockTimeRemaining.Text = remainingTime.ToString();
        }

        private void ShowScore()
        {
            gridMain.Visibility = Visibility.Hidden;

            textBlockAnswers.Text = $"Answers: {numberOfAnswers}";
            textBlockCorrectAnswers.Text = $"Correct nswers: {numberOfCorrectAnswers}";

            gridScore.Visibility = Visibility.Visible;
        }
    }
}
