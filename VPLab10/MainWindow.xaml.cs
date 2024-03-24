using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                writer.WriteLine($"{userName}|{numberOfCorrectAnswers}|{numberOfAnswers - numberOfCorrectAnswers}");
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
