using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace SpaceShooter
{
    public partial class Form1 : Form
    {
        bool isColliding = false;
        int speed = 10;
        int normalSpeed = 10;
        int boostedSpeed = 20;
        Random random = new Random();

        int playerHP = 100;
        int score = 0;
        ProgressBar hpBar;
        Label scoreLabel;
        Label loseLabel;
        Button restartButton;

        bool leftPressed, rightPressed, upPressed, downPressed;

        enum PowerUpType { Speed, Shield, Medkit }
        class PowerUpInfo
        {
            public PictureBox PictureBox;
            public PowerUpType Type;
        }

        class BlasterInfo
        {
            public PictureBox PictureBox;
        }

        class AsteroidInfo
        {
            public PictureBox PictureBox;
            public float Angle;
        }

        class EnemyInfo
        {
            public PictureBox PictureBox;
            public Timer ShootTimer;
            public bool MovingRight = true; 
        }

        class EnemyBulletInfo
        {
            public PictureBox PictureBox;
        }

        class BossInfo
        {
            public PictureBox PictureBox;
            public int HP;
            public ProgressBar HPBar;
            public bool MovingRight;
            public Timer ShootTimer;
        }

        List<Image> animationFrame;
        int currentFrameIndex = 0;
        Timer animeTimer;

        Image player;
        Image asteroidImage = Image.FromFile("asteroid.png");
        int count_asteroid = 0;
         List<Point> asteroids = new List<Point>();


        List<BlasterInfo> blasters = new List<BlasterInfo>();
        //List<AsteroidInfo> asteroids = new List<AsteroidInfo>();
        List<EnemyInfo> enemies = new List<EnemyInfo>();
        List<EnemyBulletInfo> enemyBullets = new List<EnemyBulletInfo>();
        List<PowerUpInfo> powerUps = new List<PowerUpInfo>();
        BossInfo boss = null;
        bool bossActive = false;

        Timer gameTimer = new Timer();
        Timer asteroidTimer = new Timer();
        Timer enemyTimer = new Timer();
        Timer shieldTimer = new Timer();
        Timer speedTimer = new Timer();

        bool shieldActive = false;
        Image playerNormalImage = Properties.Resources.player_unscreen;
        Image playerShieldImage = Properties.Resources.player_unscreenR;


        private int imageX = 50;
        private int imageY = 50;
        private int speed_ship = 5;

        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            

            // HP Bar
            hpBar = new ProgressBar();
            hpBar.Maximum = 100;
            hpBar.Value = playerHP;
            hpBar.Width = 200;
            hpBar.Height = 20;
            hpBar.Location = new Point(10, 30);
            hpBar.ForeColor = Color.Red;
            this.Controls.Add(hpBar);

            // Score label
            scoreLabel = new Label();
            scoreLabel.Text = "Очки: 0";
            scoreLabel.Font = new Font("Arial", 14, FontStyle.Bold);
            scoreLabel.ForeColor = Color.Yellow;
            scoreLabel.AutoSize = true;
            scoreLabel.BackColor = Color.Transparent;
            scoreLabel.Location = new Point(hpBar.Right + 20, 30);
            this.Controls.Add(scoreLabel);

            // Lose label
            loseLabel = new Label();
            loseLabel.Text = "Вы проиграли!";
            loseLabel.Font = new Font("Arial", 32, FontStyle.Bold);
            loseLabel.ForeColor = Color.Red;
            loseLabel.AutoSize = true;
            loseLabel.Visible = false;
            loseLabel.BackColor = Color.Transparent;
            loseLabel.Location = new Point(this.ClientSize.Width / 2 - 200, this.ClientSize.Height / 2 - 100);
            loseLabel.Anchor = AnchorStyles.None;
            this.Controls.Add(loseLabel);

            // Restart button
            restartButton = new Button();
            restartButton.Text = "Начать заново";
            restartButton.Font = new Font("Arial", 16, FontStyle.Bold);
            restartButton.Size = new Size(200, 50);
            restartButton.Visible = false;
            restartButton.Location = new Point(this.ClientSize.Width / 2 - 100, this.ClientSize.Height / 2);
            restartButton.Click += RestartButton_Click;
            this.Controls.Add(restartButton);

            gameTimer.Interval = 20; // ~50 FPS
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            asteroidTimer.Interval = 1000;
            asteroidTimer.Tick += (s, a) => Asteroid();
            asteroidTimer.Start();

            enemyTimer.Interval = 3000;
            enemyTimer.Tick += (s, a) => { if (!bossActive) SpawnEnemy(); };
            enemyTimer.Start();

            shieldTimer.Interval = 10000;
            shieldTimer.Tick += (s, e) => DeactivateShield();
            speedTimer.Interval = 10000;
            speedTimer.Tick += (s, e) => DeactivateSpeed();

            this.KeyUp += new KeyEventHandler(Form1_KeyUp);
            this.KeyPreview = true;
            this.Shown += (s, e) => this.Focus();
          
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (!gameTimer.Enabled) return;

            if (e.KeyCode == Keys.A) leftPressed = true;
            if (e.KeyCode == Keys.D) rightPressed = true;
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up) upPressed = true;
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down) downPressed = true;
            if (e.KeyCode == Keys.Space) Shoot();
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A) leftPressed = false;
            if (e.KeyCode == Keys.D) rightPressed = false;
            if (e.KeyCode == Keys.W || e.KeyCode == Keys.Up) upPressed = false;
            if (e.KeyCode == Keys.S || e.KeyCode == Keys.Down) downPressed = false;
        }

        public void Shoot()
        {
            if (!gameTimer.Enabled) return;

            PictureBox blaster = new PictureBox();
          
            blaster.Width = 40;   
            blaster.Height = 80;  
            blaster.SizeMode = PictureBoxSizeMode.StretchImage;
            blaster.Image = Properties.Resources.laser1;
            blaster.Name = "Blaster";
            blaster.BackColor = Color.Transparent;
            blaster.Left = imageX + 30;
            blaster.Top = imageY - 30;
            this.Controls.Add(blaster);
            blasters.Add(new BlasterInfo { PictureBox = blaster });
        }

        public void Asteroid()
        {
            if (!gameTimer.Enabled) return;
            if (asteroids.Count <= 10)
            {
                int x = random.Next(0, this.ClientSize.Width - 64);
                int y = 0;
                PictureBox asteroid = new PictureBox();

                asteroids.Add(new Point(x, y));
            }


        }

        public void SpawnEnemy()
        {
            if (!gameTimer.Enabled) return;
            PictureBox enemy = new PictureBox();
            enemy.Top = 0;
            enemy.Left = random.Next(0, this.ClientSize.Width - 96);
            enemy.Width = 96;
            enemy.Height = 96;
            enemy.SizeMode = PictureBoxSizeMode.StretchImage;
            enemy.Image = Properties.Resources.vrag_unscreen; 
            enemy.Name = "enemy";
            enemy.BackColor = Color.Transparent;
            this.Controls.Add(enemy);

            Timer shootTimer = new Timer();
            shootTimer.Interval = 1400;
            shootTimer.Tick += (s, e) => EnemyShoot(enemy);
         
            bool movingRight = random.Next(2) == 0;

            var info = new EnemyInfo { PictureBox = enemy, ShootTimer = shootTimer, MovingRight = movingRight };
            enemies.Add(info);
            shootTimer.Start();
        }

        public void EnemyShoot(PictureBox enemy)
        {
            PictureBox bullet = new PictureBox();
            bullet.Width = 32;  
            bullet.Height = 64;  
            bullet.SizeMode = PictureBoxSizeMode.StretchImage;
            bullet.Image = Properties.Resources.laser2;
            bullet.BackColor = Color.Transparent;
            bullet.Left = enemy.Left + enemy.Width / 2 - bullet.Width / 2;
            bullet.Top = enemy.Top + enemy.Height;
            this.Controls.Add(bullet);
            enemyBullets.Add(new EnemyBulletInfo { PictureBox = bullet });
        }


        public void SpawnBoss()
        {
            bossActive = true;
            PictureBox bossPic = new PictureBox();
            bossPic.Top = 30;
            bossPic.Left = (this.ClientSize.Width - 192) / 2;
            bossPic.Width = 192;
            bossPic.Height = 192;
            bossPic.SizeMode = PictureBoxSizeMode.StretchImage;
            bossPic.Image = Properties.Resources.boss_unscreen;
            bossPic.BackColor = Color.Transparent;
            this.Controls.Add(bossPic);

            ProgressBar bossBar = new ProgressBar();
            bossBar.Maximum = 10;
            bossBar.Value = 10;
            bossBar.Width = 200;
            bossBar.Height = 20;
            bossBar.Location = new Point(this.ClientSize.Width / 2 - 100, bossPic.Top - 25);
            this.Controls.Add(bossBar);

            Timer shootTimer = new Timer();
            shootTimer.Interval = 1000;
            shootTimer.Tick += (s, e) => BossShoot(bossPic);

            boss = new BossInfo
            {
                PictureBox = bossPic,
                HP = 10,
                HPBar = bossBar,
                MovingRight = true,
                ShootTimer = shootTimer
            };
            shootTimer.Start();
        }

        public void BossShoot(PictureBox bossPic)
        {
            PictureBox bullet = new PictureBox();
            bullet.Width = 40;   
            bullet.Height = 80; 
            bullet.SizeMode = PictureBoxSizeMode.StretchImage;
            bullet.Image = Properties.Resources.laser3;
            bullet.BackColor = Color.Transparent;
            bullet.Left = bossPic.Left + bossPic.Width / 2 - bullet.Width / 2;
            bullet.Top = bossPic.Top + bossPic.Height;
            this.Controls.Add(bullet);
            enemyBullets.Add(new EnemyBulletInfo { PictureBox = bullet });
        }


        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            foreach (Point asteroidPos in asteroids)
            {
                e.Graphics.DrawImage(asteroidImage, asteroidPos.X, asteroidPos.Y, 64, 64);
            }
        }


        private void CheckCollision()
        {



            Rectangle playerRect = new Rectangle(imageX, imageY, player.Width, player.Height);


      
                // Столкновение с астероидами
                for (int i = asteroids.Count - 1; i >= 0; i--)
            {
                {

                    Rectangle asteroidRect = new Rectangle(asteroids[i].X, asteroids[i].Y, 64, 64);
                    if (playerRect.IntersectsWith(asteroidRect))
                    {
                        isColliding = true;
                        DecreaseHP(10);
                        RemoveScore(5);
                        asteroids.RemoveAt(i);
                        break;
                    }
                    

                    for (int j = 0; j < blasters.ToList().Count; j++)
                    {
                        var blaster = blasters[j];
                        blaster.PictureBox.Top -= 5;

                        if (blaster.PictureBox.Bounds.IntersectsWith(asteroidRect))
                        {
                            blasters.RemoveAt(j);
                            blaster.PictureBox.Top = -200;
                            blasters.Remove(blaster);
                            blaster.PictureBox = null;
                            asteroids.RemoveAt(i);
                            break;
                        }
                    }
                }

                // Столкновение с вражескими пулями
                foreach (var bullet in enemyBullets.ToList())
                {
                    if (playerRect.IntersectsWith(bullet.PictureBox.Bounds))
                    {
                        this.Controls.Remove(bullet.PictureBox);
                        enemyBullets.Remove(bullet);
                        DecreaseHP(10);
                        break;
                    }
                }

                // Столкновение с врагами
                foreach (var enemy in enemies.ToList())
                {
                    if (playerRect.IntersectsWith(enemy.PictureBox.Bounds))
                    {
                        this.Controls.Remove(enemy.PictureBox);
                        enemy.ShootTimer.Stop();
                        enemies.Remove(enemy);
                        DecreaseHP(20);
                        break;
                    }
                }

                // Столкновение с бонусами
                foreach (var powerUp in powerUps.ToList())
                {
                    if (playerRect.IntersectsWith(powerUp.PictureBox.Bounds))
                    {
                        this.Controls.Remove(powerUp.PictureBox);
                        powerUps.Remove(powerUp);

                        switch (powerUp.Type)
                        {
                            case PowerUpType.Medkit:
                                playerHP += 20;
                                if (playerHP > 100) playerHP = 100;
                                hpBar.Value = playerHP;
                                break;
                            case PowerUpType.Shield:
                                ActivateShield();
                                break;
                            case PowerUpType.Speed:
                                ActivateSpeed();
                                break;
                        }
                    }
                }
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            // Движение игрока по состоянию клавиш
            if (leftPressed) imageX -= speed;
            if (rightPressed) imageX += speed;
            if (upPressed) imageY -= speed;
            if (downPressed) imageY += speed;

            CheckCollision();

            for (int i = 0; i < asteroids.Count; i++)
            {
                Point pos = asteroids[i];
                pos.Y += 2; // скорость падения

                // Сброс за нижним краем
                if (pos.Y > this.ClientSize.Height)
                {
                    pos.Y = random.Next(-500, -150);
                    pos.X = random.Next(0, this.ClientSize.Width);
                }

                asteroids[i] = pos;
            }


            // --- внутри GameLoop после движения астероидов ---
            for (int i = blasters.Count - 1; i >= 0; i--)
            {
                var blasterInfo = blasters[i];
                var blaster = blasterInfo.PictureBox;
                blaster.Top -= 15;

                // Удаляем лазер, если вылетел за экран
                if (blaster.Bottom < 0)
                {
                    Controls.Remove(blaster);
                    blasters.RemoveAt(i);
                    continue;
                }

                // Проверка попадания по астероидам
                for (int j = asteroids.Count - 1; j >= 0; j--)
                {
                    var pos = asteroids[j];
                    var rectAst = new Rectangle(pos.X, pos.Y, 64, 64);
                    if (blaster.Bounds.IntersectsWith(rectAst))
                    {
                        // сбили астероид
                        Controls.Remove(blaster);
                        blasters.RemoveAt(i);
                        asteroids.RemoveAt(j);

                        AddScore(5);    // +5 за астероид
                        break;
                    }
                }

                // Проверка попадания по врагам
                foreach (var enemy in enemies.ToList())
                {
                    if (blaster.Bounds.IntersectsWith(enemy.PictureBox.Bounds))
                    {
                        // убрали лазер и врага
                        Controls.Remove(blaster);
                        blasters.RemoveAt(i);

                        Controls.Remove(enemy.PictureBox);
                        enemy.ShootTimer.Stop();
                        enemies.Remove(enemy);

                        AddScore(20);   // +20 за корабль
                        break;
                    }
                }

                // Проверка попадания по боссу
                if (bossActive && boss != null && blaster.Bounds.IntersectsWith(boss.PictureBox.Bounds))
                {
                    Controls.Remove(blaster);
                    blasters.RemoveAt(i);

                    boss.HP--;
                    boss.HPBar.Value = boss.HP;
                    if (boss.HP <= 0)
                    {
                        Controls.Remove(boss.PictureBox);
                        Controls.Remove(boss.HPBar);
                        boss.ShootTimer.Stop();
                        boss = null;
                        bossActive = false;

                        AddScore(100);  // +100 за босса
                    }
                }
            }

            Invalidate();

            // Движение врагов
            foreach (var enemy in enemies.ToList())
            {
                enemy.PictureBox.Top += 3;
                
                if (enemy.PictureBox.Top > this.ClientSize.Height)
                {
                    this.Controls.Remove(enemy.PictureBox);
                    enemy.ShootTimer.Stop();
                    enemies.Remove(enemy);
                }
            }

            // Движение вражеских пуль
            foreach (var bullet in enemyBullets.ToList())
            {
                bullet.PictureBox.Top += 10;
                
                if (bullet.PictureBox.Top > this.ClientSize.Height)
                {
                    this.Controls.Remove(bullet.PictureBox);
                    enemyBullets.Remove(bullet);
                }
            }

            // Движение и стрельба босса
            if (bossActive && boss != null)
            {
                if (boss.MovingRight)
                    boss.PictureBox.Left += 5;
                else
                    boss.PictureBox.Left -= 5;

                if (boss.PictureBox.Left <= 0)
                    boss.MovingRight = true;
                if (boss.PictureBox.Right >= this.ClientSize.Width)
                    boss.MovingRight = false;
            }

            // Появление босса
            if (!bossActive && score > 0 && score % 150 == 0)
            {
                SpawnBoss();
            }
        }

        private void ActivateSpeed()
        {
            speed = boostedSpeed;
            speedTimer.Stop();
            speedTimer.Start();
        }
        private void DeactivateSpeed()
        {
            speed = normalSpeed;
            speedTimer.Stop();
        }
        private void ActivateShield()
        {
            shieldActive = true;
           // playerPictureBox.Image = playerShieldImage;
            shieldTimer.Stop();
            shieldTimer.Start();
        }
        private void DeactivateShield()
        {
            shieldActive = false;
            //playerPictureBox.Image = playerNormalImage;
            shieldTimer.Stop();
        }

        private void AddScore(int amount)
        {
            score += amount;
            scoreLabel.Text = $"Очки: {score}";
        }

        private void RemoveScore(int amount)
        {
            score -= amount;
            if (score < 0) score = 0;
            scoreLabel.Text = $"Очки: {score}";
        }

        private void DecreaseHP(int amount)
        {
            if (shieldActive) return;
            playerHP -= amount;
            if (playerHP < 0) playerHP = 0;
            hpBar.Value = playerHP;

            if (playerHP == 0)
                GameOver();
        }

        private void GameOver()
        {
            gameTimer.Stop();
            asteroidTimer.Stop();
            enemyTimer.Stop();
            foreach (var enemy in enemies) enemy.ShootTimer.Stop();
            if (boss != null) boss.ShootTimer.Stop();
            loseLabel.Visible = true;
            restartButton.Visible = true;
        }

        private void RestartButton_Click(object sender, EventArgs e)
        {
            foreach (var blaster in blasters)
                this.Controls.Remove(blaster.PictureBox);
            foreach (var asteroid in asteroids)
               // this.Controls.Remove(asteroid.PictureBox);
            foreach (var enemy in enemies)
            {
                this.Controls.Remove(enemy.PictureBox);
                enemy.ShootTimer.Stop();
            }
            foreach (var bullet in enemyBullets)
                this.Controls.Remove(bullet.PictureBox);
            foreach (var pu in powerUps)
                this.Controls.Remove(pu.PictureBox);
            if (boss != null)
            {
                this.Controls.Remove(boss.PictureBox);
                this.Controls.Remove(boss.HPBar);
                boss.ShootTimer.Stop();
            }
            blasters.Clear();
            asteroids.Clear();
            enemies.Clear();
            enemyBullets.Clear();
            powerUps.Clear();
            boss = null;
            bossActive = false;

            playerHP = 100;
            hpBar.Value = playerHP;
            score = 0;
            scoreLabel.Text = "Очки: 0";

            loseLabel.Visible = false;
            restartButton.Visible = false;

           // playerPictureBox.Left = (this.ClientSize.Width - playerPictureBox.Width) / 2;
           // playerPictureBox.Top = this.ClientSize.Height - playerPictureBox.Height - 30;

            shieldActive = false;
           // playerPictureBox.Image = playerNormalImage;
            speed = normalSpeed;
            shieldTimer.Stop();
            speedTimer.Stop();

            this.Focus();

            gameTimer.Start();
            asteroidTimer.Start();
            enemyTimer.Start();
        }

        private void фоноваяМузВыклToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ToolStripMenuItemBgSound.Tag.ToString() == "On")
            {
                Options.bg_player.Stop();
                ToolStripMenuItemBgSound.Text = "Фоновая музыка вкл";
                ToolStripMenuItemBgSound.Tag = "Off";
            }
            else
            {
                Options.bg_player.Play();
                ToolStripMenuItemBgSound.Text = "Фоновая музыка выкл";
                ToolStripMenuItemBgSound.Tag = "On";
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {

            gameTimer.Stop();

            FormSettings form = new FormSettings();
            form.FormClosed += Form_FormClosed;
            form.Show();
        }

        private void Form_FormClosed(object sender, FormClosedEventArgs e)
        {

            gameTimer.Start();
        }

       

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeAnimation();
           
            Options.bg_player = new SoundPlayer(@"Sounds/bg_music.wav");
             Options.bg_player.Play();
            this.Focus();
        }

        private void InitializeAnimation()
        {
            animationFrame = new List<Image>();
            animationFrame.Add(Image.FromFile("Ship/Sprite1.png"));
            animationFrame.Add(Image.FromFile("Ship/Sprite2.png"));
            animationFrame.Add(Image.FromFile("Ship/Sprite3.png"));
            animationFrame.Add(Image.FromFile("Ship/Sprite4.png"));
            animationFrame.Add(Image.FromFile("Ship/Sprite5.png"));

            animeTimer = new Timer();
            animeTimer.Interval = 100;
            animeTimer.Tick += AnimeTimer_Tick;
            animeTimer.Start();
            
            //this.panel1.DoubleBuffered = true;
            
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            if (animationFrame != null && currentFrameIndex < animationFrame.Count)
            {
                if (currentFrameIndex == 1)
                {

                }
                player = animationFrame[currentFrameIndex];
              

                e.Graphics.DrawImage(player, imageX, imageY, player.Width, player.Height);
            }
        }

        

        private void AnimeTimer_Tick(object sender, EventArgs e)
        {
            currentFrameIndex++;

            if (currentFrameIndex >= animationFrame.Count) 
            { 
                currentFrameIndex = 0;
            }
            
        }

        private void ShowExplosion(Point location, Size size)
        {

        }

    }
}