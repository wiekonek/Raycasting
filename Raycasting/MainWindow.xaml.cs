using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Raycasting
{
  enum WallSide { NS, EW }

  public partial class MainWindow
  {
    private Timer aTimer;

    private Vector playerPosition = new Vector(22, 12);
    private Vector playerDirection = new Vector(-1, 0);
    private Vector plane = new Vector(0, 0.66);

    private const int mapWidth = 24;
    private const int mapHeight = 24;
    private readonly int[,] worldMap = new int[mapWidth, mapHeight]
    {
      {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
      {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,0,0,0,0,0,2,2,2,2,2,0,0,0,0,3,0,3,0,3,0,0,0,1},
      {1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,3,0,0,0,3,0,0,0,1},
      {1,0,0,0,0,0,2,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,0,0,0,0,0,2,2,0,2,2,0,0,0,0,3,0,3,0,3,0,0,0,1},
      {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,4,4,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,4,0,4,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,4,0,0,0,0,5,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,4,0,4,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,4,0,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,4,4,4,4,4,4,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1},
      {1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1},
    };
    private readonly Dictionary<int, Color> wallColors =
      new Dictionary<int, Color>()
      {
        { 1, Colors.Black },
        { 2, Colors.Blue },
        { 3, Colors.Green },
        { 4, Colors.Red },
        { 5, Colors.Yellow },
      };


    public MainWindow()
    {
      InitializeComponent();

      aTimer = new Timer { Interval = 1000/10 };
      aTimer.Start();
      aTimer.Elapsed += OnTimedEvent;

      KeyDown += new KeyEventHandler(OnButtonKeyDown);
    }

    private void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
      Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Action(DrawScene));
    }

    private void DrawScene()               
    { 
      MainCanvas.Children.Clear();
      for (int x = 0; x < Width; x += 4 )      
      {                                    
        var cameraX = 2 * x / (double)Width - 1;
        var rayPosition = playerPosition;  
        var rayDirection = (playerDirection + plane) * cameraX;
                                           
        var mapBoxX = (int)playerPosition.X;     
        var mapBoxY = (int)playerPosition.Y;

        var delta = new Vector()
        {
          X = Math.Sqrt(1 + (rayDirection.Y * rayDirection.Y) / (rayDirection.X * rayDirection.X)),
          Y = Math.Sqrt(1 + (rayDirection.X * rayDirection.X) / (rayDirection.Y * rayDirection.Y)),
        };


        int stepX, stepY;
        var sideDistance = new Vector();


        if (rayDirection.X < 0)
        {
          stepX = -1;
          sideDistance.X = (rayPosition.X - mapBoxX) * delta.X;
        }
        else
        {
          stepX = 1;
          sideDistance.X = (mapBoxX + 1.0 - rayPosition.X) * delta.X;
        }
        if (rayDirection.Y < 0)
        {
          stepY = -1;
          sideDistance.Y = (rayPosition.Y - mapBoxY) * delta.Y;
        }
        else
        {
          stepY = 1;
          sideDistance.Y = (mapBoxY + 1.0 - rayPosition.Y) * delta.Y;
        }

        var hit = false;
        var side = WallSide.NS;
        
        while (!hit)
        {
          //jump to next map square, OR in x-direction, OR in y-direction
          if (delta.X < delta.Y)
          {
            sideDistance.X += delta.X;
            mapBoxX += stepX;
            side = WallSide.NS;
          }
          else
          {
            sideDistance.Y += delta.Y;
            mapBoxY += stepY;
            side = WallSide.EW;
          }
          //Check if ray has hit a wall
          if (worldMap[mapBoxX, mapBoxY] > 0) hit = true;
        }

        double wallDistance;

        if(side == WallSide.NS)
        {
          wallDistance = (mapBoxX - rayPosition.X + (1 - stepX) / 2) / rayDirection.X;
        }
        else
        {
          wallDistance = (mapBoxY - rayPosition.Y + (1 - stepY) / 2) / rayDirection.Y;
        }



        //Calculate height of line to draw on screen
        int lineHeight = (int)(Height / wallDistance);

        //calculate lowest and highest pixel to fill in current stripe
        int drawStart = -lineHeight / 2 + (int)Height / 2;
        if (drawStart < 0) drawStart = 0;
        int drawEnd = lineHeight / 2 + (int)Height / 2;
        if (drawEnd >= (int)Height) drawEnd = (int)Height - 1;


        DrawHorizontalLine(x, drawStart, drawEnd, wallColors[worldMap[mapBoxX, mapBoxY]]);
      }
    }

    private void OnButtonKeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Down:
          break;
        case Key.Up:
          break;
        case Key.Left:
          playerDirection.X++;
          break;
        case Key.Right:
          playerDirection.Y++;
          break;
      }
    }

    private void DrawHorizontalLine(int x, int start, int end, Color color)
    {
      for (int h = start; h < end; h+= 4)
      {
        PutPixel(x, h, new SolidColorBrush(color));
      }
    }

    private void DrawLine(Vector start, Vector end, Color color)
    {
      var line = new Line()
      {
        X1 = start.X,
        X2 = end.X,
        Y1 = start.Y,
        Y2 = end.Y,
        Fill = new SolidColorBrush(color)
      };
      MainCanvas.Children.Add(line);
    }

    private void PutPixel(int x, int y, Brush color)
    {
      var rec = new Rectangle();
      Canvas.SetTop(rec, y);
      Canvas.SetLeft(rec, x);
      rec.Width = 4;
      rec.Height = 4;
      rec.Fill = color;
      MainCanvas.Children.Add(rec);
    }
  }
}
