using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpaceGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int Lifes = 3;
        int LifeBarWith = 100;
        int Score = 0;

        Random random = new Random();

        int speed = 8;
        bool Moveleft,Moveright;

        //Skotten
        BitmapImage shotTexture = new BitmapImage(new Uri("pack://application:,,,/Images/Shot.png"));
        List<Image> Shots = new List<Image>();
        int ShotTimer, ShotDelay = 15;

        //Fienden
        BitmapImage enemyTexture = new BitmapImage(new Uri("pack://application:,,,/Images/Enemy.png"));
        List<Image> Enemies = new List<Image>();
        int EnemyTimer,EnemyDelay = 25;

        //Livpaket
        BitmapImage parcelTexture = new BitmapImage(new Uri("pack://application:,,,/Images/Parcel.png"));
        List<Image> Parcels = new List<Image>();
        int ParcelTimer = 200;

        //Explosion
        BitmapImage explosionTexture = new BitmapImage(new Uri("pack://application:,,,/Images/Explosion.png"));
        List<Image> Explosions = new List<Image>();

        //Ljud
        MediaPlayer LaserPlayer = new MediaPlayer();
        MediaPlayer ExplosionPlayer = new MediaPlayer();
        MediaPlayer WindPlayer = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            MoveShip();
            MoveShots();
            CreateEnemy();
            MoveEnemies();
            CheckCollissions();
            CreateParcel();
            MoveParcel();
            FadeExplosion();

            ShowScore();
        }

        private void CreateParcel()
        {
            ParcelTimer--;
            if (ParcelTimer < 0)
            {
                PlayWind();
                ParcelTimer = random.Next(150, 500);
                Image parcel = new Image() { Height = 60, Width = 60, Source = parcelTexture };
                Canvas.SetTop(parcel, -60);
                Canvas.SetLeft(parcel, random.Next(10,1100));

                GameCanvas.Children.Add(parcel);
                Parcels.Add(parcel);
            }
        }

        private void MoveParcel()
        {
            Rect ShipRect = new Rect(Canvas.GetLeft(SpaceShip), Canvas.GetTop(SpaceShip), SpaceShip.Width, SpaceShip.Height);
            foreach (var parcel in Parcels)
            {
                Rect ParcelRect = new Rect(Canvas.GetLeft(parcel), Canvas.GetTop(parcel), parcel.Width, parcel.Height);
                Canvas.SetTop(parcel, Canvas.GetTop(parcel) + 4);
                if (Canvas.GetTop(parcel) > 800)
                {
                    parcel.Visibility = Visibility.Hidden;
                    GameCanvas.Children.Remove(parcel);
                }

                if (ParcelRect.IntersectsWith(ShipRect))
                {
                    LifeBarWith += 20;
                    if (LifeBarWith >= 200)
                    {
                        Lifes++;
                        LifeBarWith = 100;
                    }
                    parcel.Visibility = Visibility.Hidden;
                    GameCanvas.Children.Remove(parcel);
                }


            }
            Parcels.RemoveAll(x=>x.Visibility == Visibility.Hidden);
            
        }

        private void CreateExplosion(double top,double left)
        {
            PlayExplosion();
            Image explosion = new Image() { Height=80,Width=80, Source=explosionTexture};
            Canvas.SetLeft(explosion, left);
            Canvas.SetTop(explosion, top);
            GameCanvas.Children.Add(explosion);
            Explosions.Add(explosion);
        }

        private void FadeExplosion()
        {
            foreach(var explosion in Explosions)
            {
                explosion.Opacity -= 0.02;
                if(explosion.Opacity < 0.04)
                {
                    GameCanvas.Children.Remove(explosion);
                }
            }
            Explosions.RemoveAll(x => x.Opacity < 0.04);
        }

        private void CheckCollissions()
        {
            Rect ShipRect = new Rect(Canvas.GetLeft(SpaceShip), Canvas.GetTop(SpaceShip), SpaceShip.Width, SpaceShip.Height);

            //Loopa igenom våra fiender
            foreach (var enemy in Enemies)
            {
                Rect EnemyRect = new Rect(Canvas.GetLeft(enemy), Canvas.GetTop(enemy), enemy.Width, enemy.Height);
                if (EnemyRect.IntersectsWith(ShipRect))
                {
                    Lifes--;
                    enemy.Visibility = Visibility.Hidden;
                    GameCanvas.Children.Remove(enemy);
                    CreateExplosion(EnemyRect.Top, EnemyRect.Left);
                }

                foreach(var shot in Shots)
                {
                    Rect ShotRect = new Rect(Canvas.GetLeft(shot), Canvas.GetTop(shot), shot.Width, shot.Height);
                    if (EnemyRect.IntersectsWith(ShotRect)) 
                    {
                        //Öka score med 1
                        Score++;

                        //Skapa explosion
                        CreateExplosion(EnemyRect.Top, EnemyRect.Left);
                        //Ta bort fiende från spelplanen
                        enemy.Visibility = Visibility.Hidden;
                        GameCanvas.Children.Remove(enemy);

                        //radera skottet från spelplanen
                        shot.Visibility = Visibility.Hidden;
                        GameCanvas.Children.Remove(shot);
                    }
                }

            }
            //radera osylinga objekt ur minnet / listorna
            Enemies.RemoveAll(x=>x.Visibility== Visibility.Hidden);
            Shots.RemoveAll(x=>x.Visibility== Visibility.Hidden);
        }

        private void ShowScore()
        {
            if (LifeBarWith == 0 && Lifes > 0) 
            {
                Lifes--;
                LifeBarWith = 100;
            }
            if (Lifes == 0)
            {
                PlayWind();
                //Gameover!!
                CompositionTarget.Rendering -= GameLoop;
                GameOverCanvas.Visibility = Visibility.Visible;
            }
            TxtLife.Text = "Liv: " + Lifes;
            TxtScore.Text = "Poäng: " + Score;
            Lifebar.Width = LifeBarWith;
        }
       
        private void CreateEnemy()
        {
            EnemyTimer--;
            if(EnemyTimer< 0)
            {
                EnemyTimer = EnemyDelay;
                Image enemy = new Image() { Height = 50, Width = 50 };
                Canvas.SetTop(enemy, -50);
                Canvas.SetLeft(enemy, random.Next(10, 1100));
                enemy.Source = enemyTexture;
                GameCanvas.Children.Add(enemy);
                Enemies.Add(enemy);
            }
            
        }

        private void MoveEnemies()
        {
            foreach (var enemy in Enemies)
            {
                Canvas.SetTop(enemy, Canvas.GetTop(enemy) + 5);
                if (Canvas.GetTop(enemy) > 700)
                {
                    enemy.Visibility = Visibility.Hidden;
                    GameCanvas.Children.Remove(enemy);
                    LifeBarWith -= 1;
                }
            }
            Enemies.RemoveAll(x=> x.Visibility == Visibility.Hidden);
        }

        private void CreateShot()
        {
            PlayLaser();
            Image shot = new Image() { Height = 20, Width = 8 };
            Canvas.SetLeft(shot, Canvas.GetLeft(SpaceShip) + 41);
            Canvas.SetTop(shot, Canvas.GetTop(SpaceShip) + 10);
            
            //Sätter grafiken
            shot.Source = shotTexture;

            //Lägg till på skärmen
            GameCanvas.Children.Add(shot);

            //Lägg till i Listan
            Shots.Add(shot);
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering += GameLoop;
            StartCanvas.Visibility = Visibility.Hidden;
        }

        private void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            PlayWind();
            GameOverCanvas.Visibility = Visibility.Hidden;
            CompositionTarget.Rendering += GameLoop;
            Score = 0;
            LifeBarWith = 100;
            Lifes = 3;
            
            foreach(var enemy in Enemies)
            {
                GameCanvas.Children.Remove(enemy);
            }
            foreach(var shot in Shots)
            {
                GameCanvas.Children.Remove(shot);
            }
            Shots.Clear();
            Enemies.Clear();
        }

        private void MoveShots()
        {
            ShotTimer--;
            foreach (Image shot in Shots)
            {
                Canvas.SetTop(shot,Canvas.GetTop(shot) - 5);
                if (Canvas.GetTop(shot) < -20)
                {
                    shot.Visibility = Visibility.Hidden;
                    GameCanvas.Children.Remove(shot);
                }
            }
            //Radera alla osynliga skott
            Shots.RemoveAll(x=> x.Visibility == Visibility.Hidden);          

        }

        private void MoveShip()
        {
            double ShipLeft = Canvas.GetLeft(SpaceShip);
            if (Moveright == true && ShipLeft < 1090)
            {
                Canvas.SetLeft(SpaceShip, ShipLeft+speed);
            }
            if (Moveleft == true && ShipLeft>10)
            {
                Canvas.SetLeft(SpaceShip, ShipLeft - speed);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key== Key.Left)
            {
                Moveleft = true;
                Moveright = false;
            }
            if (e.Key == Key.Right)
            {
                Moveleft = false;
                Moveright = true;
            }
            if(e.Key == Key.Space)
            {
                if (ShotTimer < 0)
                {
                    ShotTimer = ShotDelay;
                    CreateShot();
                }
                
            }
        }

        private void PlayLaser()
        {
            LaserPlayer.Open(new Uri(@"Sounds\laserSound.wav", UriKind.Relative));
            LaserPlayer.Play();
        }
        private void PlayExplosion()
        {
            ExplosionPlayer.Open(new Uri(@"Sounds\explosionSound.wav", UriKind.Relative));
            ExplosionPlayer.Play();
        }
        private void PlayWind()
        {
            WindPlayer.Open(new Uri(@"Sounds\wind.wav", UriKind.Relative));
            WindPlayer.Play();
        }

    }
}