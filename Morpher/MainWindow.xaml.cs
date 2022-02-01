using System;
using System.Collections.Generic;
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
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;

namespace Morpher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapSource[] frames;
        private int frameIndex = 0;
        private DispatcherTimer timer;
        public bool playing = false;
        public bool reversing = false;
        public MainWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow.WindowState = WindowState.Maximized;
            source.PreviewMouseLeftButtonUp += Source_PreviewMouseLeftButtonUp;
            dest.PreviewMouseLeftButtonUp += Dest_PreviewMouseLeftButtonUp;
            ToggleMorphSettings(false);
        }
        private void AddLine(ControlLineCanvas canvas1, ControlLineCanvas canvas2)
        {
            if (canvas1.IsDrawing)
            {
                canvas1.IsDrawing = false;
                var cl1 = canvas1.ControlLines[canvas1.ControlLines.Count - 1];
                var cl2 = new ControlLine(canvas2, cl1);
                canvas2.ControlLines.Add(cl2);
                cl1.Mid.PreviewMouseRightButtonDown += DeleteLine;
                cl2.Mid.PreviewMouseRightButtonDown += DeleteLine;
            }
        }

        private void Reset()
        {
            frameIndex = 0;
            result.Source = frames[0];
        }

        private void Source_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AddLine(source, dest);
        }
        private void Dest_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AddLine(dest, source);
        }
        private void DeleteLine(object sender, MouseButtonEventArgs e)
        {
            var m = sender as ControlLineMidThumb;
            int index = m.ControlLine.Index;
            source.RemoveAt(index);
            dest.RemoveAt(index);
            Console.WriteLine(source.ControlLines.Count);
            Console.WriteLine(dest.ControlLines.Count);
        }
        private void sourceButton_Click(object sender, RoutedEventArgs e)
        {
            if (source.SetImage() == true)
            {

                sourceButton.IsEnabled = false;
                destButton.IsEnabled = true;
            }
        }

        private void ToggleMorphSettings(bool isEnabled)
        {
            clearButton.IsEnabled = isEnabled;
            morphButton.IsEnabled = isEnabled;
            aSetting.IsEnabled = isEnabled;
            bSetting.IsEnabled = isEnabled;
            pSetting.IsEnabled = isEnabled;
            startButton.IsEnabled = isEnabled;
            nextFrameButton.IsEnabled = isEnabled;
            midFrameButton.IsEnabled = isEnabled;
            prevFrameButton.IsEnabled = isEnabled;
            endButton.IsEnabled = isEnabled;
            playButton.IsEnabled = isEnabled;
            reverseButton.IsEnabled = isEnabled;
            numFramesSetting.IsEnabled = isEnabled;
            numThreadsSetting.IsEnabled = isEnabled;
        }

        private void destButton_Click(object sender, RoutedEventArgs e)
        {
            if (dest.SetImage(source.BitmapSource.PixelWidth, source.BitmapSource.PixelHeight) == true)
            {
                destButton.IsEnabled = false;
                source.CanDraw = true;
                dest.CanDraw = true;
                aSetting.IsEnabled = true;
                bSetting.IsEnabled = true;
                pSetting.IsEnabled = true;
                morphButton.IsEnabled = true;
                numFramesSetting.IsEnabled = true;
                numThreadsSetting.IsEnabled = true;
            }
        }

        private async void playButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleMorphSettings(true);
            /*timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 33);
            timer.Start();*/
            if (!playing)
            {
                playButton.Content = "Pause";
                playing = true;
                //setImageSrc(Src);
                UpdateLayout();
                await Task.Run(() =>
                {
                    Thread.Sleep(1000 / frames.Length);
                });
                for (; frameIndex < frames.Length; frameIndex++)
                {
                    if (playing)
                    {
                        result.Source = frames[frameIndex];
                        UpdateLayout();
                        await Task.Run(() =>
                        {
                            Thread.Sleep(1000 / frames.Length);
                        });
                    }
                    else
                    {
                        break;
                    }
                }
                if (playing)
                {
                    frameIndex = frames.Length - 1;
                    result.Source = frames[frameIndex];
                    UpdateLayout();
                    playButton.Content = "Play";
                }
            }
            else
            {
                playButton.Content = "Play";
                playing = false;
            }

        }
        private async void reverseButton_Click(object sender, RoutedEventArgs e)
        {
            if (!reversing)
            {
                reverseButton.Content = "Pause";
                reversing = true;
                UpdateLayout();
                await Task.Run(() =>
                {
                    Thread.Sleep(1000 / frames.Length);
                });
                for (; frameIndex >= 0; frameIndex--)
                {
                    if (reversing)
                    {
                        result.Source = frames[frameIndex];
                        UpdateLayout();
                        await Task.Run(() =>
                        {
                            Thread.Sleep(1000 / frames.Length);
                        });
                    }
                    else
                    {
                        break;
                    }
                }

                if (reversing)
                {
                    frameIndex = 0;
                    result.Source = frames[frameIndex];
                    reverseButton.Content = "Reverse";
                    UpdateLayout();
                }
            }
            else
            {
                reverseButton.Content = "Reverse";
                reversing = false;
            }
        }
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            frameIndex = 0;
            result.Source = frames[frameIndex];
            UpdateLayout();
        }
        private void prevFrameButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(frameIndex);
            if (frameIndex > 0)
            {
                result.Source = frames[--frameIndex];
            }
        }
        private void midFrameButton_Click(object sender, RoutedEventArgs e)
        {
            frameIndex = frames.Length / 2;
            result.Source = frames[frameIndex];
            UpdateLayout();
        }

        private void nextFrameButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(frameIndex);
            if (frameIndex < frames.Length - 1)
            {
                result.Source = frames[++frameIndex];
            }
        }
        private void endButton_Click(object sender, RoutedEventArgs e)
        {
            frameIndex = frames.Length - 1;
            result.Source = frames[frameIndex];
            UpdateLayout();
        }
        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            source.Clear();
            dest.Clear();
            source.CanDraw = false;
            dest.CanDraw = false;
            source.image.Source = null;
            dest.image.Source = null;
            result.Source = null;
            sourceButton.IsEnabled = true;
            destButton.IsEnabled = false;
            startButton.IsEnabled = false;
            endButton.IsEnabled = false;
            frameIndex = 0;

            ToggleMorphSettings(false);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (++frameIndex < frames.Length)
            {
                result.Source = frames[frameIndex];
            }
            else
            {
                timer.Stop();
                ToggleMorphSettings(true);
            }
        }

        private async void morphButton_Click(object sender, RoutedEventArgs e)
        {
            if (source.image == null || dest.image == null || source.ControlLines.Count == 0)
            {
                MessageBoxResult mbr = MessageBox.Show("You can't create a morph unless both source" +
                    "and destination images are set and there is at least one control line pair drawn", "",
                                MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            else
            {
                ToggleMorphSettings(false);

                frameIndex = 0;
                DirectBitmap resultBitmap = new DirectBitmap(source.DirectBitmap.Width, source.DirectBitmap.Height);

                double a = Convert.ToDouble(aSetting.Text);
                double b = Convert.ToDouble(bSetting.Text);
                double p = Convert.ToDouble(pSetting.Text);
                int numFrames = Convert.ToInt32(numFramesSetting.Text);
                int numFramesLeft = numFrames;
                int numThreads = Convert.ToInt32(numThreadsSetting.Text);

                frames = new BitmapSource[numFrames];
                Task[] tasks = new Task[numThreads];

                Morphers morpher = new Morphers(source, dest, numFrames, a, b, p);

                if (numFrames < 1)
                    return;

                Stopwatch stopwatch = Stopwatch.StartNew();

                for (int i = 0; i < numThreads; i++)
                {
                    int numThreadFrames = (i < numThreads - 1) ? (numFramesLeft / numThreads) : numFramesLeft;
                    int begin = numFrames - numFramesLeft;
                    tasks[i] = Task.Run(() =>
                    {
                        for (int j = begin; j < begin + numThreadFrames; j++)
                        {
                            BitmapSource frame = morpher.GenerateFrame(j);
                            frame.Freeze();
                            frames[j] = frame;
                        }
                    });
                    numFramesLeft -= numThreadFrames;
                }

                await Task.WhenAll(tasks);

                stopwatch.Stop();
                benchmark.Content = "Benchmark: " + stopwatch.ElapsedMilliseconds + "ms";

                Console.WriteLine("done");
                result.Source = frames[0];
                ToggleMorphSettings(true);
            }
        }


        /*  private void ASlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
          {
              aSetting.Text = Convert.ToString(ASlider.Value);
          }*/
    }
}
