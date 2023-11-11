
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using Graphics = Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Microsoft.VisualBasic.Devices;
using Mouse = Microsoft.Xna.Framework.Input.Mouse;
using Keyboard = Microsoft.Xna.Framework.Input.Keyboard;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D9;

namespace DivEdit
{
    public class Game1 : Game
    {
        public bool IAmActive { get; set; }
        private GraphicsDeviceManager _graphics;
        private Graphics.SpriteBatch _spriteBatch;
        Texture2D MenuPointerUp;
        Texture2D MenuPointerDown;
        Texture2D MenuLine;
        Texture2D Menu;
        Texture2D Main;
        Texture2D MenuTextures;
        Texture2D MenuObjects;
        Texture2D MenuMonster;
        static Texture2D SelectObj;
        Texture2D ConsoleBack;
        Texture2D point;
        Texture2D monster;
        Texture2D texturesFrame;
        Texture2D[,] TilesGreedFull = new Texture2D[60, 34];
        Texture2D[,] TilesGreedHalf = new Texture2D[60, 34];
        Texture2D exampleTexture_0, exampleTexture_1, exampleTexture_2, exampleTexture_3;
        SpriteFont textBlock;
        // ----Таймеры для кнопок------
        long timer_1 = 0;
        long timer_2 = 0;
        long timer_3 = 0;
        long timer_4 = 0;
        // ------Пути к файлам---------
        String config = @"config.ini";
        String mainDirect;
        String savegameDirect;
        String fileExtension;
        String WorldFile = "F:\\SteamLibrary\\steamapps\\common\\divine_divinity\\savegames\\test_1\\world.x0";
        String ObjFile = "F:\\SteamLibrary\\steamapps\\common\\divine_divinity\\savegames\\test_1\\objects.x0";
        String DataFile;
        String Editor = @"editor.dat";
        String objectsIhfoFile = @"objects.de";
        //-----------------------------
        readonly GameData GD = new();
        static ObjectsInfo[] objectsInfo = new ObjectsInfo[11264]; //Массив объектов класса ObjectsInfo с описанием объектов
        List<Metaobject> metobj = new();
        List<Button> button = new();
        List<int[]> eggs = new();
        String[] Text = { "", "", "", "", "", "", "", "", "", "" }; //Массив текста консоли
        ushort[] widths; // Разрешение экрана  по ширине  
        ushort[] heights; // Разрешение экрана  по высоте
        Keys[] FunctionKeys = new Keys[] { Keys.F1, Keys.F2, Keys.F3, Keys.F4, Keys.F5, Keys.F6 }; // Клавиши выбора разрешения
        int xCor = 0; //Координата экрана
        int yCor = 0; //Координата экрана
        bool showObjects = true; //Показать объекты
        bool showConsole = true;
        bool showTexturesFrame = false;
        bool showTileEffect = false;
        bool showEggs = false;
        bool objectWork = false;
        bool textureWork = false;
        int selectedObject = -1; //Выбранный объект
        int selectedEgg = -1; //Выбранный монстр
        int textures = 0; //Выбранная текстура
        int obj = 0;
        //-----------------------------
        bool fullScreen = false;
        bool mouseLBState = false; //Текущее состояние левой кнопки мыши
        bool mouseLBOldState = false; //Предыдущее состояние левой кнопки мыши
        bool mouseRBState = false; //Текущее состояние правой кнопки мыши
        bool mouseRBOldState = false; //Предыдущее состояние правой кнопки мыши
        int cursorOffsetX = 0;
        int cursorOffsetY = 0;
        int mouseXCorOld = 0;
        int mouseYCorOld = 0;
        int ScrollWheelOldValue = 0;
        bool movingAnObject = false; //Процесс перемещения объекта
        bool movingAnEgg = false; //Процесс перемещения монстра
        //====================================================================================================================================================
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferredBackBufferWidth = 1152;
            _graphics.PreferredBackBufferHeight = 832;
            _graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            this.Activated += ActivateMyGame;
            this.Deactivated += DeactivateMyGame;
            this.IAmActive = false;
        }
        //====================================================================================================================================================
        [STAThread]
        protected override void Initialize()
        {
            bool configExist = File.Exists(config);
            if (configExist)
            {
                string[] lines = File.ReadAllLines(config);
                foreach (string s in lines)
                {
                    mainDirect = lines[1];
                    savegameDirect = lines[3];
                    fileExtension = lines[5];
                }
            }
            else Exit();
            WorldFile = savegameDirect + "\\world" + fileExtension;
            ObjFile = savegameDirect + "\\objects" + fileExtension;
            DataFile = savegameDirect + "\\data.000";
            GD.Initialize(WorldFile, ObjFile);
            objectsInfo = ReadAndWriteFile.readObjectsInfo(objectsIhfoFile);
            eggs = ReadAndWriteFile.ReadEggs(DataFile);
            widths = new ushort[] { 768, 1024, 1152, 1280, 1600, 1920 }; //Массив выбора разрешений экрана
            heights = new ushort[] { 576, 768, 832, 704, 896, 1088 }; //Массив выбора разрешений экрана
            metobj = ReadAndWriteFile.ReadMetaobject(Editor);
            MenuPointerUp = Content.Load<Texture2D>("images/000022");
            MenuPointerDown = Content.Load<Texture2D>("images/000034");
            MenuLine = Content.Load<Texture2D>("images/000087");
            Menu = Content.Load<Texture2D>("images/icon_menu");
            MenuMonster = Content.Load<Texture2D>("images/icon_monster");
            Main = Content.Load<Texture2D>("images/Main");
            MenuTextures = Content.Load<Texture2D>("images/icon_textures");
            MenuObjects = Content.Load<Texture2D>("images/icon_objects");
            ConsoleBack = Content.Load<Texture2D>("images/000003a");
            point = Content.Load<Texture2D>("images/point");
            monster = Content.Load<Texture2D>("images/monster");
            texturesFrame = Content.Load<Texture2D>("images/textures_frame");
            base.Initialize();
        }
        //====================================================================================================================================================
        [STAThread]
        protected override void LoadContent()
        {
            _spriteBatch = new Graphics.SpriteBatch(GraphicsDevice);
            textBlock = Content.Load<SpriteFont>("text");
            update_tile_and_objects();
            button.Add(new Button(16, Window.ClientBounds.Height - 246, true, MenuPointerDown, "")); // Скрыть консоль
            button.Add(new Button(16, Window.ClientBounds.Height - 30, false, MenuPointerUp, "")); // Показать консоль
            button.Add(new Button(16, 16, true, Menu, "")); // Меню
            button.Add(new Button(16, 103, true, MenuTextures, "")); // Текстуры
            button.Add(new Button(16, 190, true, MenuObjects, "")); // Объекты
            button.Add(new Button(16, 277, true, MenuMonster, "")); // Монстры
        }
        //====================================================================================================================================================
        [STAThread]
        protected override void Update(GameTime gameTime)
        {
            if (IAmActive)
            {
                KeyboardAndMouseHandler();
                update_tile_and_objects();
                base.Update(gameTime);
            }
        }
        //====================================================================================================================================================
        [STAThread]
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            RenderingTextures();
            if (showObjects) RenderingObjects();
            if (showEggs) RenderingEggs();
            if (showConsole) ConsoleShow(16, Window.ClientBounds.Height - 246);
            showMenu();
            if (showTexturesFrame) showTextures();
            _spriteBatch.End();
            base.Draw(gameTime);
        }
        //====================================================================================================================================================
        //====================================================================================================================================================
        //====================================================================================================================================================
        //====================================================================================================================================================
        //====================================================================================================================================================
        //====================================================================================================================================================
        public void ActivateMyGame(object sendet, EventArgs args)
        {
            IAmActive = true;
        }
        public void DeactivateMyGame(object sendet, EventArgs args)
        {
            IAmActive = false;
        }
        public void KeyboardAndMouseHandler() //Обработчик клавиатуры и мыши
        {
            bool menuSelected = false;
            bool eventHandledAlready = false;
            MouseState currentMouseState = Mouse.GetState();
            KeyboardState keyboardState = Keyboard.GetState();
            if (currentMouseState.LeftButton == ButtonState.Pressed) mouseLBState = true;
            else mouseLBState = false;
            if (currentMouseState.RightButton == ButtonState.Pressed) mouseRBState = true;
            else mouseRBState = false;

            if (button[0].Click(currentMouseState.X, currentMouseState.Y, mouseLBState) && !eventHandledAlready)
            {
                if (Stopwatch.GetTimestamp() - timer_3 > 3000000)
                {
                    showConsole = false;
                    button[0].visibility = false;
                    button[1].visibility = true;
                    eventHandledAlready = true;
                    timer_3 = Stopwatch.GetTimestamp();
                }
                menuSelected = true;
            }
            if (button[1].Click(currentMouseState.X, currentMouseState.Y, mouseLBState) && !eventHandledAlready)
            {
                if (Stopwatch.GetTimestamp() - timer_3 > 3000000)
                {
                    showConsole = true;
                    button[0].visibility = true;
                    button[1].visibility = false;
                    eventHandledAlready = true;
                    timer_3 = Stopwatch.GetTimestamp();
                }
                menuSelected = true;
            }
            if (button[2].Click(currentMouseState.X, currentMouseState.Y, mouseLBState) && !eventHandledAlready) //Menu
            {
                if (Stopwatch.GetTimestamp() - timer_3 > 3000000)
                {
                    button[2].selected = false;
                    button[3].selected = false;
                    button[4].selected = false;
                    objectWork = false;
                    textureWork = false;
                    showTexturesFrame = false;
                    Process proc = Process.Start("notepad.exe", @"readme.txt");
                    proc.WaitForExit();
                    proc.Close();
                    System.Diagnostics.Debug.WriteLine("Файл закрыт");
                    eventHandledAlready = true;
                    timer_3 = Stopwatch.GetTimestamp();
                }
                menuSelected = true;
            }
            if (button[3].Click(currentMouseState.X, currentMouseState.Y, mouseLBState) && !eventHandledAlready) //Textures
            {
                if (Stopwatch.GetTimestamp() - timer_3 > 3000000)
                {
                    objectWork = false;
                    textureWork = !textureWork;
                    showEggs = false;
                    button[5].selected = false;
                    button[3].selected = textureWork;
                    button[4].selected = false;
                    showTexturesFrame = textureWork;
                    selectedObject = -1;
                    eventHandledAlready = true;
                    timer_3 = Stopwatch.GetTimestamp();
                }
                menuSelected = true;
            }
            if (button[4].Click(currentMouseState.X, currentMouseState.Y, mouseLBState) && !eventHandledAlready) //Objects
            {
                if (Stopwatch.GetTimestamp() - timer_3 > 3000000)
                {
                    objectWork = !objectWork;
                    textureWork = false;
                    showEggs = false;
                    button[5].selected = false;
                    button[4].selected = objectWork;
                    button[3].selected = false;
                    showTexturesFrame = false;
                    selectedObject = -1;
                    eventHandledAlready = true;
                    timer_3 = Stopwatch.GetTimestamp();
                }
                menuSelected = true;
            }
            if (button[5].Click(currentMouseState.X, currentMouseState.Y, mouseLBState) && !eventHandledAlready) //Monsters
            {
                if (Stopwatch.GetTimestamp() - timer_3 > 3000000)
                {
                    showEggs = !showEggs;
                    objectWork = false;
                    textureWork = false;
                    button[5].selected = showEggs;
                    button[4].selected = false;
                    button[3].selected = false;
                    button[2].selected = false;
                    showTexturesFrame = false;
                    selectedObject = -1;
                    eventHandledAlready = true;
                    timer_3 = Stopwatch.GetTimestamp();
                }
                menuSelected = true;
            }
            //Редактировать плитку ПКМ
            if (mouseRBState && showTexturesFrame && !objectWork && !showEggs && Stopwatch.GetTimestamp() - timer_4 > 2000000)
            {
                int x = xCor + currentMouseState.X / 64;
                int y = yCor + currentMouseState.Y / 64;
                if (x >= 0 && x < 512 && y >= 0 && y < 1024)
                {
                    int[] tile = GD.GetTile(x, y);
                    string result = Microsoft.VisualBasic.Interaction.InputBox("full Textures_half Textures_tileEffect_var1_var2", "", tile[0].ToString().PadLeft(5, '0') + "_" +
                                   tile[1].ToString().PadLeft(5, '0') + "_" +
                                   tile[2].ToString().PadLeft(5, '0') + "_" +
                                   tile[3].ToString().PadLeft(5, '0') + "_" +
                                   tile[4].ToString().PadLeft(5, '0'));
                    if (result.Length == 29)
                    {
                        string[] words = result.Split(new char[] { '_' });
                        int[] newTile = new int[5];
                        Int32.TryParse(words[0], out newTile[0]);
                        Int32.TryParse(words[1], out newTile[1]);
                        Int32.TryParse(words[2], out newTile[2]);
                        Int32.TryParse(words[3], out newTile[3]);
                        Int32.TryParse(words[4], out newTile[4]);
                        GD.SetTile(newTile, x, y);
                    }
                }
                timer_4 = Stopwatch.GetTimestamp();
            }
            //Редактировать объект ПКМ
            if (selectedObject >= 0 && mouseRBState && !showTexturesFrame && objectWork && !showEggs && Stopwatch.GetTimestamp() - timer_4 > 2000000)
            {
                int[] obj2 = GD.GetObjectInFrame(selectedObject);
                int[] obj = GD.GetObject(obj2[8]);
                string result = Microsoft.VisualBasic.Interaction.InputBox("Object " + obj2[8], "",
                    obj[00].ToString() + "_" + obj[01].ToString() + "_" + obj[02].ToString() + "_" + obj[03].ToString() + "_" + obj[04].ToString() + "_" +
                    obj[05].ToString() + "_" + obj[06].ToString() + "_" + obj[07].ToString() + "_" + obj[08].ToString() + "_" + obj[09].ToString() + "_" +
                    obj[10].ToString() + "_" + obj[11].ToString() + "_" + obj[12].ToString() + "_" + obj[13].ToString() + "_" + obj[14].ToString() + "_" +
                    obj[15].ToString() + "_" + obj[16].ToString() + "_" + obj[17].ToString() + "_" + obj[18].ToString() + "_" + obj[19].ToString() + "_" +
                    obj[20].ToString() + "_" + obj[21].ToString() + "_" + obj[22].ToString() + "_" + obj[23].ToString() + "_" + obj[24].ToString());
                if (result.Length > 0)
                {
                    string[] words = result.Split(new char[] { '_' });
                    int[] newobj = new int[25];
                    for (int i = 0; i < 25; i++)
                    {
                        Int32.TryParse(words[i], out newobj[i]);
                    }
                    GD.SetObject(obj2[8], newobj);
                }
                timer_4 = Stopwatch.GetTimestamp();
            }
            //Инфо о плитке
            if (mouseLBState && !showTexturesFrame && !objectWork && !showEggs && Stopwatch.GetTimestamp() - timer_4 > 2000000)
            {
                int x = xCor + currentMouseState.X / 64;
                int y = yCor + currentMouseState.Y / 64;
                if (x >=0 && x < 512 && y >= 0 && y < 1024)
                {
                    int[] tile = GD.GetTile(x, y);
                    ConsoleAdd(tile[0].ToString().PadLeft(6, '0') + " " +
                               tile[1].ToString().PadLeft(6, '0') + " " +
                               tile[2].ToString().PadLeft(6, '0') + " " +
                               tile[3].ToString().PadLeft(6, '0') + " " +
                               tile[4].ToString().PadLeft(6, '0') + " ");
                }
                timer_4 = Stopwatch.GetTimestamp();
            }
            //Выбор текстуры колесиком мыши
            if (showTexturesFrame && (currentMouseState.Y > 16) && (currentMouseState.Y < 144) && (currentMouseState.X > 102) && (currentMouseState.X < 230))
            {
                if (currentMouseState.ScrollWheelValue > ScrollWheelOldValue && textures < Terrain.TotalCount() - 1) textures++;
                if (currentMouseState.ScrollWheelValue < ScrollWheelOldValue && textures > 0) textures--;
            }
            //Выбор объекта
            if (!menuSelected && objectWork && mouseLBState && mouseLBOldState == false && !eventHandledAlready && Stopwatch.GetTimestamp() - timer_4 > 2000000)
            {
                int x = currentMouseState.X;
                int y = currentMouseState.Y;
                int objCount = GD.GetObjectCount();
                for (int i = objCount - 1; i >= 0; i--)
                {
                    int[] checkedObject = GD.GetObjectInFrame(i);
                    if (checkCursorInSprite(checkedObject, x, y))
                    {
                        Color[,] colors2D = getColorArray(i);
                        if (colors2D[x - checkedObject[4], y - checkedObject[5] + checkedObject[6]].A != 0)
                        {
                            selectedObject = i;
                            int[] obj2 = GD.GetObjectInFrame(selectedObject);
                            int[] obj = GD.GetObject(obj2[8]);
                            ConsoleAdd("Object " + obj2[8]);
                            ConsoleAdd(obj[00].ToString() + "_" + obj[01].ToString() + "_" + obj[02].ToString() + "_" + obj[03].ToString() + "_" + obj[04].ToString() + "_" +
                                obj[05].ToString() + "_" + obj[06].ToString() + "_" + obj[07].ToString() + "_" + obj[08].ToString() + "_" + obj[09].ToString() + "_" +
                                obj[10].ToString() + "_" + obj[11].ToString() + "_" + obj[12].ToString());
                            ConsoleAdd(obj[13].ToString() + "_" + obj[14].ToString() + "_" +
                                obj[15].ToString() + "_" + obj[16].ToString() + "_" + obj[17].ToString() + "_" + obj[18].ToString() + "_" + obj[19].ToString() + "_" +
                                obj[20].ToString() + "_" + obj[21].ToString() + "_" + obj[22].ToString() + "_" + obj[23].ToString() + "_" + obj[24].ToString());
                            break;
                        }
                    }
                    selectedObject = -1;
                }
                timer_4 = Stopwatch.GetTimestamp();
            }
            //Выбор монстра
            if (!menuSelected && showEggs && mouseLBState && mouseLBOldState == false && !eventHandledAlready && Stopwatch.GetTimestamp() - timer_4 > 2000000)
            {
                int x = currentMouseState.X + 30;
                int y = currentMouseState.Y + 32;
                int Xsize = 59;
                int Ysize = 64;
                Color[] ColorArray = new Color[Xsize * Ysize];
                monster.GetData(ColorArray);
                Color[,] colors2D = new Color[Xsize, Ysize];
                for (int i = eggs.Count - 1; i >= 0; i--)
                {
                    int eggsXCor = eggs[i][0] - xCor * 64;
                    int eggsYCor = eggs[i][1] - yCor * 64;
                    if (x > eggsXCor && x < eggsXCor + Xsize && y > eggsYCor && y < eggsYCor + Ysize)
                    {
                        for (int r = 0; r < Xsize; r++)
                        {
                            for (int t = 0; t < Ysize; t++)
                            {
                                colors2D[r, t] = ColorArray[r + t * Xsize];
                            }
                        }
                        if (colors2D[x - eggsXCor, y - eggsYCor].A != 0)
                        {
                            selectedEgg = i;
                            break;
                        }
                    }
                    selectedEgg = -1;
                    ConsoleClear();
                }
                timer_4 = Stopwatch.GetTimestamp();
            }
            //Удаление объекта
            if (!menuSelected && selectedObject >= 0 && keyboardState.IsKeyDown(Keys.Delete))
            {
                selectedObject = GD.ObjectDel(selectedObject);
            }
            //Удаление монстра
            if (!menuSelected && selectedEgg >= 0 && keyboardState.IsKeyDown(Keys.Delete))
            {
                eggs.RemoveRange(selectedEgg, 1);
                ConsoleClear();
                ConsoleAdd("Egg " + selectedEgg + " delete");
                selectedEgg = -1;
            }
            //Начало перемещения объекта
            if (!menuSelected && selectedObject >= 0 && mouseLBOldState && !movingAnObject && Stopwatch.GetTimestamp() - timer_4 > 1500000)
            {
                movingAnObject = GD.StartMovingAnObject(selectedObject, currentMouseState.X, currentMouseState.Y, xCor, yCor, keyboardState.IsKeyDown(Keys.LeftControl));
                selectedObject = -1;
                if (movingAnObject != true)
                {
                    cursorOffsetX = 0;
                    cursorOffsetY = 0;
                }
                timer_4 = Stopwatch.GetTimestamp();
            }
            //Начало перемещения монстра
            if (!menuSelected && showEggs && selectedEgg >= 0 && mouseLBOldState && !movingAnEgg && Stopwatch.GetTimestamp() - timer_4 > 1500000)
            {
                int x1 = currentMouseState.X + 30;
                int y1 = currentMouseState.Y + 32;
                int Xsize = 59;
                int Ysize = 64;
                //Если курсор в области выделенного объекта
                int eggsXCor = eggs[selectedEgg][0] - xCor * 64;
                int eggsYCor = eggs[selectedEgg][1] - yCor * 64;
                if (x1 > eggsXCor && x1 < eggsXCor + Xsize && y1 > eggsYCor && y1 < eggsYCor + Ysize)
                {
                    if (keyboardState.IsKeyDown(Keys.LeftControl))
                    {
                        eggs.Add(new int[15]);
                        for (int k = 0; k < 15; k++)
                        {
                            eggs[eggs.Count - 1][k] = eggs[selectedEgg][k];
                        }
                        eggs[eggs.Count - 1][12] = eggs.Count - 1;
                        selectedEgg = eggs.Count - 1;
                    }
                    cursorOffsetX = currentMouseState.X - eggsXCor;
                    cursorOffsetY = currentMouseState.Y - eggsYCor;

                    movingAnEgg = true;
                }
                else
                {
                    movingAnEgg = false;
                    selectedEgg = -1;
                    cursorOffsetX = 0;
                    cursorOffsetY = 0;
                }
                timer_4 = Stopwatch.GetTimestamp();
            }
            //Перемещение объекта
            if (!menuSelected && movingAnObject && mouseLBOldState && mouseLBState)
            {
                GD.MovingAnObject(keyboardState.IsKeyDown(Keys.LeftShift), currentMouseState.X, currentMouseState.Y);
            }
            //Перемещение монстра
            if (!menuSelected && showEggs && movingAnEgg && mouseLBOldState && mouseLBState && selectedEgg >=0)
            {
                eggs[selectedEgg][0] = xCor * 64 + currentMouseState.X - cursorOffsetX;
                eggs[selectedEgg][1] = yCor * 64 + currentMouseState.Y - cursorOffsetY;
            }
            //Конец перемещения монстра
            if (!menuSelected && showEggs && movingAnEgg && mouseLBOldState && !mouseLBState)
            {
                movingAnEgg = false;
            }
            //Вставка объекта после перемещения
            if (!menuSelected && movingAnObject && mouseLBOldState && !mouseLBState)
            {
                movingAnObject = GD.PasteObjectAfterMove(xCor, yCor, currentMouseState.X, currentMouseState.Y, keyboardState.IsKeyDown(Keys.LeftShift));
            }
            //Изменение высоты объекта
            if (!menuSelected && selectedObject >= 0 && (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.Down)))
            {
                GD.ChangingHeightObject(selectedObject, keyboardState.IsKeyDown(Keys.Up));
            }
            //Наложение текстуры
            if (!menuSelected && textureWork && mouseLBState && (currentMouseState.Y / 64 + yCor) > 0 && (currentMouseState.X / 64 + xCor) > 0 && !eventHandledAlready && Stopwatch.GetTimestamp() - timer_4 > 100000)
            {
                GD.TextureMapping(textures, currentMouseState.X, currentMouseState.Y, xCor, yCor, keyboardState.IsKeyDown(Keys.LeftShift), keyboardState.IsKeyDown(Keys.LeftControl));
                timer_4 = Stopwatch.GetTimestamp();
            }
            //Сдвиг экрана мышью
            if (currentMouseState.X < 15 && fullScreen && Stopwatch.GetTimestamp() - timer_1 > 100000)
            {
                if (xCor > 1) xCor--;
                selectedObject = -1;
            }
            if (currentMouseState.X > Window.ClientBounds.Width - 15 && fullScreen && Stopwatch.GetTimestamp() - timer_1 > 100000)
            {
                if (xCor < 511 - Window.ClientBounds.Width / 64) xCor++;
                selectedObject = -1;
            }
            if (currentMouseState.Y < 50 && fullScreen && Stopwatch.GetTimestamp() - timer_1 > 100000)
            {
                if (yCor > 1) yCor--;
                selectedObject = -1;
            }
            if (currentMouseState.Y > Window.ClientBounds.Height - 15 && fullScreen && Stopwatch.GetTimestamp() - timer_1 > 100000)
            {
                if (yCor < 1023 - Window.ClientBounds.Height / 64) yCor++;
                selectedObject = -1;
            }
            timer_1 = Stopwatch.GetTimestamp();
            //Выбор разрешения экрана
            for (byte i = 0; i < FunctionKeys.Length; i++)
            {
                if (Keyboard.GetState().IsKeyDown(FunctionKeys[i]))
                {
                    fullScreen = false;
                    _graphics.IsFullScreen = fullScreen;
                    ChangeResolution(i);
                    if (xCor > 511 - Window.ClientBounds.Width / 64) xCor = 511 - Window.ClientBounds.Width / 64;
                    if (yCor > 1023 - Window.ClientBounds.Height / 64) yCor = 1023 - Window.ClientBounds.Height / 64;
                    update_tile_and_objects();
                    selectedObject = -1;
                }
            }
            //Полный экран
            if (keyboardState.IsKeyDown(Keys.F8) && Stopwatch.GetTimestamp() - timer_2 > 2500000)
            {
                fullScreen = !fullScreen;
                _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                _graphics.IsFullScreen = fullScreen;
                _graphics.ApplyChanges();
                selectedObject = -1;
                timer_2 = Stopwatch.GetTimestamp();
            }
            //Выход из приложения
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                Exit();
            //Сдвиг экрана влево клавишами A D S W
            if (keyboardState.IsKeyDown(Keys.A))
            {
                if (xCor > 1) xCor--;
                selectedObject = -1;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                if (xCor < 511 - Window.ClientBounds.Width / 64) xCor++;
                selectedObject = -1;
            }
            if (keyboardState.IsKeyDown(Keys.W))
            {
                if (yCor > 1) yCor--;
                selectedObject = -1;
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                if (yCor < 1023 - Window.ClientBounds.Height / 64) yCor++;
                selectedObject = -1;
            }
            //Читать мир R
            if (keyboardState.IsKeyDown(Keys.R) && Stopwatch.GetTimestamp() - timer_2 > 2500000)
            {
                GD.Initialize(WorldFile, ObjFile);
                eggs = ReadAndWriteFile.ReadEggs(DataFile);
                ConsoleAdd("Read warld and objects");
                timer_2 = Stopwatch.GetTimestamp();
            }
            //Записать мир T
            if (keyboardState.IsKeyDown(Keys.T) && Stopwatch.GetTimestamp() - timer_2 > 2500000)
            {
                ReadAndWriteFile.writeObjects_2(ObjFile, GD.GetObjects());
                ReadAndWriteFile.writeWorld(WorldFile, GD.GetTiles());
                ReadAndWriteFile.WriteEggs(DataFile, eggs);
                ConsoleAdd("Save warld and objects");
                timer_2 = Stopwatch.GetTimestamp();
            }
            // Q 
            if (keyboardState.IsKeyDown(Keys.Q) && Stopwatch.GetTimestamp() - timer_2 > 50000)
            {
                //ConsoleClear();
                //ConsoleAdd(obj + "  metaobject: " + metobj[obj].metaobject);
                //if (metobj[obj].group != null) ConsoleAdd("group: " + metobj[obj].group);
                //if (metobj[obj].location != null) ConsoleAdd("location: " + metobj[obj].location);
                //ConsoleAdd("size: " + metobj[obj].size[0] + " " + metobj[obj].size[1]);
                //int[] a = new int[4];
                //for (int i = 0; i < metobj[obj].Object.Count; i++) 
                //{
                //    a = metobj[obj].Object[i];
                //    ConsoleAdd(a[0].ToString() + " " + a[1].ToString() + " " + a[2].ToString() + " " + a[3].ToString());
                //}
                //if (metobj[obj].type != null) ConsoleAdd("type: " + metobj[obj].type);
                //if (obj < Metaobject.TotalCount() - 1) obj++;
                //else obj = 0;
                //obj++;

                //string result = Microsoft.VisualBasic.Interaction.InputBox("Введите текст:", "tile", "555");

                //Form1 F = new Form1();
                string result = Microsoft.VisualBasic.Interaction.InputBox("2 4 6 8 10 12 20 28 36 64 66 68 72 74 76 80 82 84 88 90 92 94 96 100", "tile", "");
                int num;
                Int32.TryParse(result, out num);
                for (int x = 0; x < 512; x++)
                {
                    for (int y = 0; y < 1024; y++)
                    {
                        int[] tile = GD.GetTile(x, y);
                        if  (tile[2] == num && x < 480 && y < 1000)
                        {
                            xCor = x;
                            yCor = y;
                            break;
                        }
                    }
                }
                

                timer_2 = Stopwatch.GetTimestamp();
            }
            // E
            if (keyboardState.IsKeyDown(Keys.E) && Stopwatch.GetTimestamp() - timer_2 > 2500000)
            {
                showTileEffect = !showTileEffect;
                timer_2 = Stopwatch.GetTimestamp();
            }
            if (currentMouseState.LeftButton == ButtonState.Pressed) mouseLBOldState = true;
            else mouseLBOldState = false;
            if (currentMouseState.RightButton == ButtonState.Pressed) mouseRBOldState = true;
            else mouseRBOldState = false;
            if (currentMouseState.LeftButton == ButtonState.Released) movingAnObject = false;
            mouseXCorOld = currentMouseState.X;
            mouseYCorOld = currentMouseState.Y;
            ScrollWheelOldValue = currentMouseState.ScrollWheelValue;
            button[0].yCor = Window.ClientBounds.Height - 246; //Обновляем положение кнопки после изменения размера окна
            button[1].yCor = Window.ClientBounds.Height - 30; //Обновляем положение кнопки после изменения размера окна
        }
        //====================================================================================================================================================
        public void RenderingTextures()//Выводим текстуры плитки
        {
            for (int i = 0; i < Window.ClientBounds.Width / 64; i++)
            {
                for (int j = 0; j < Window.ClientBounds.Height / 64 + 1; j++)
                {
                    int effect = GD.GetTileEffect(i + xCor, j + yCor);
                    if (showTileEffect && effect != 0)
                    {
                        // 0 2 4 6 8 10 12 20 28 36 64 66 68 72 74 76 80 82 84 88 90 92 94 96 100
                        if (effect == 2)//Вода
                        {
                            _spriteBatch.Draw(TilesGreedFull[i, j], new Vector2(i * 64, j * 64), Color.Gold);
                            _spriteBatch.Draw(TilesGreedHalf[i, j], new Vector2(i * 64, j * 64), Color.Gold);
                        }
                        if (effect == 4)//Помещение
                        {
                            _spriteBatch.Draw(TilesGreedFull[i, j], new Vector2(i * 64, j * 64), Color.Maroon);
                            _spriteBatch.Draw(TilesGreedHalf[i, j], new Vector2(i * 64, j * 64), Color.Maroon);
                        }
                        if (effect == 8)//Туман
                        {
                            _spriteBatch.Draw(TilesGreedFull[i, j], new Vector2(i * 64, j * 64), Color.Aqua);
                            _spriteBatch.Draw(TilesGreedHalf[i, j], new Vector2(i * 64, j * 64), Color.Aqua);
                        }
                        if (effect == 10)//Вода и туман
                        {
                            _spriteBatch.Draw(TilesGreedFull[i, j], new Vector2(i * 64, j * 64), Color.Red);
                            _spriteBatch.Draw(TilesGreedHalf[i, j], new Vector2(i * 64, j * 64), Color.Red);
                        }
                        if (effect > 10)
                        {
                            _spriteBatch.Draw(TilesGreedFull[i, j], new Vector2(i * 64, j * 64), Color.Silver);
                            _spriteBatch.Draw(TilesGreedHalf[i, j], new Vector2(i * 64, j * 64), Color.Silver);
                        }
                    }
                    else
                    {
                        _spriteBatch.Draw(TilesGreedFull[i, j], new Vector2(i * 64, j * 64), Color.White);
                        _spriteBatch.Draw(TilesGreedHalf[i, j], new Vector2(i * 64, j * 64), Color.White);
                    }
                }
            }
        }
        public void RenderingObjects()// Выводим объекты
        {
            for (int i = 0; i < GD.GetObjectCount(); i++)
            {
                if (i == selectedObject) _spriteBatch.Draw(Content.Load<Texture2D>(GD.GetObjectPath(i)), GD.GetObjectPosition(i), Color.Yellow);
                else _spriteBatch.Draw(Content.Load<Texture2D>(GD.GetObjectPath(i)), GD.GetObjectPosition(i), Color.White);
            }
        }
        public void RenderingEggs()//Выводим точки спавна
        {
            for (int i = 0; i < eggs.Count; i++)
            {
                if ((".x" + eggs[i][14].ToString()) == fileExtension)
                {
                    if (eggs[i][0] > xCor*64 && eggs[i][0] < (xCor * 64 + Window.ClientBounds.Width) && eggs[i][1] > yCor * 64 && eggs[i][1] < (yCor * 64 + Window.ClientBounds.Height))
                    {
                        if (i == selectedEgg)
                        {
                            _spriteBatch.Draw(monster, new Vector2(eggs[i][0] - 30 - xCor * 64, eggs[i][1] - 32 - yCor * 64), Color.Red);
                            _spriteBatch.DrawString(textBlock, eggs[i][2].ToString(), new Vector2(eggs[i][0] - xCor * 64, eggs[i][1] - yCor * 64), Color.White);
                            _spriteBatch.DrawString(textBlock, eggs[i][12].ToString(), new Vector2(eggs[i][0] - xCor * 64, eggs[i][1] + 30 - yCor * 64), Color.White);
                        }
                        else
                        {
                            _spriteBatch.Draw(monster, new Vector2(eggs[i][0] - 30 - xCor * 64, eggs[i][1] - 32 - yCor * 64), Color.Yellow);
                            _spriteBatch.DrawString(textBlock, eggs[i][2].ToString(), new Vector2(eggs[i][0] - xCor * 64, eggs[i][1] - yCor * 64), Color.White);
                            _spriteBatch.DrawString(textBlock, eggs[i][12].ToString(), new Vector2(eggs[i][0] - xCor * 64, eggs[i][1] + 30 - yCor * 64), Color.White);
                        }
                    }
                }
            }
        }
        //====================================================================================================================================================
        // Выводим предпросмотр текстур с рамкой
        public void showTextures() 
        {
            String[] tex = GD.GetTexturePalette(textures);
            exampleTexture_0 = Content.Load<Texture2D>(tex[0]);
            exampleTexture_1 = Content.Load<Texture2D>(tex[1]);
            exampleTexture_2 = Content.Load<Texture2D>(tex[2]);
            exampleTexture_3 = Content.Load<Texture2D>(tex[3]);
            _spriteBatch.Draw(exampleTexture_0, new Vector2(103,16 ), Color.White);
            _spriteBatch.Draw(exampleTexture_1, new Vector2(167, 16), Color.White);
            _spriteBatch.Draw(exampleTexture_2, new Vector2(103, 80), Color.White);
            _spriteBatch.Draw(exampleTexture_3, new Vector2(167, 80), Color.White);
            _spriteBatch.Draw(texturesFrame, new Vector2(103, 16), Color.White);
        }
        //====================================================================================================================================================
        // Выводим меню
        public void showMenu()
        {
            for (int i = 0; i < Button.TotalCount(); i++)
            {
                if (button[i].visibility)
                {
                    if (button[i].selected)
                    {
                        _spriteBatch.Draw(button[i].texture, new Vector2(button[i].xCor, button[i].yCor), Color.Yellow);
                        _spriteBatch.DrawString(textBlock, button[i].text, new Vector2(button[i].xCor, button[i].yCor), Color.White);
                    }
                    else
                    {
                        _spriteBatch.Draw(button[i].texture, new Vector2(button[i].xCor, button[i].yCor), Color.White);
                        _spriteBatch.DrawString(textBlock, button[i].text, new Vector2(button[i].xCor, button[i].yCor), Color.White);
                    }
                }
            }
        }
        //====================================================================================================================================================
        // Добавить строку в консоль
        public void ConsoleAdd(String t)
        {
            for (int i = 9; i > 0; i--)
            {
                Text[i] = Text[i - 1];
            }
            Text[0] = t;
        }
        // Очистить консоль
        public void ConsoleClear()
        {
            for (int i = 0; i < 10; i++)
            {
                Text[i] = "";
            }
        }
        // Вывести консоль
        public void ConsoleShow(int x, int y)
        {
            _spriteBatch.Draw(ConsoleBack, new Vector2(x, y), Color.White);
            for(int i = 0; i < 10;i++)
            {
                if (Text[9 - i] != null) _spriteBatch.DrawString(textBlock, Text[9-i], new Vector2(x + 50 , y + 20 + 18 * i), Color.SlateGray);
            }
        }
        //====================================================================================================================================================
        public void update_tile_and_objects()// Обновляем отображаемые плитки и объекты
        {
            for (int i = 0; i < Window.ClientBounds.Width / 64; i++)
            {
                for (int j = 0; j < Window.ClientBounds.Height / 64 + 1; j++)
                {
                    TilesGreedFull[i, j] = Content.Load<Texture2D>(GD.GetFullTileTexturesName(i + xCor, j + yCor));
                    TilesGreedHalf[i, j] = Content.Load<Texture2D>(GD.GetHalfTileTexturesName(i + xCor, j + yCor));
                }
            }
            GD.UpdateDisplayedObjects(xCor, yCor, Window.ClientBounds.Width, Window.ClientBounds.Height, movingAnObject);
        }
        //====================================================================================================================================================
        public Color[,] getColorArray(int objNum)
        {
            int objName = GD.GetObjectName(objNum);
            SelectObj = Content.Load<Texture2D>("objects/" + objName.ToString().PadLeft(6, '0'));
            Color[] ColorArray = new Color[objectsInfo[objName].width * objectsInfo[objName].height];
            SelectObj.GetData(ColorArray);
            Color[,] colors2D = new Color[objectsInfo[objName].width, objectsInfo[objName].height];
            for (int r = 0; r < objectsInfo[objName].width; r++)
            {
                for (int t = 0; t < objectsInfo[objName].height; t++)
                {
                    colors2D[r, t] = ColorArray[r + t * objectsInfo[objName].width];
                }
            }
            return colors2D;
        }
        //====================================================================================================================================================
        public bool checkCursorInSprite(int[] obj, int x, int y)
        {
            if (x > obj[4] && 
                x < obj[4] + objectsInfo[obj[0]].width && 
                y > obj[5] - obj[6] && 
                y < obj[5] + objectsInfo[obj[0]].height - obj[6])
            {
                return true;
            }
            else return false;
        }
        //====================================================================================================================================================
        void ChangeResolution(byte newResolution)// Устанавливаем разрешение экрана
        {
            if (newResolution >= 0 && newResolution < widths.Length)
            {
                _graphics.PreferredBackBufferWidth = widths[newResolution];
                _graphics.PreferredBackBufferHeight = heights[newResolution];
                _graphics.ApplyChanges();
                System.Diagnostics.Debug.WriteLine("New resolution: {0} x {1}", _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            }
        }
        //====================================================================================================================================================
    }
    public class Button
    {
        public int xCor, yCor, height, width;
        public bool visibility;
        public bool selected;
        public Texture2D texture;
        public String text;
        static int count = 0;
        public Button(int x, int y, bool vis, Texture2D tex, string text)
        {
            count++;
            this.xCor = x;
            this.yCor = y;
            this.visibility = vis;
            this.texture = tex;
            this.height = texture.Height;
            this.width = texture.Width;
            this.text = text;
            this.selected = false;
        }
        public static int TotalCount()
        {
            return count;
        }
        public bool Click(int x, int y, bool click)
        {
            if (x > xCor && x < (xCor + width) && y > yCor && y < (yCor + height) && click)
            {
                return true;
            }
            else return false;
        }
    }
    public class Form1 : Form
    {
        public Form1() { }

    }
}
//System.Diagnostics.Debug.WriteLine(line);

//Form texturesForm = new Form();
//texturesForm.SetBounds(600, 300, 500, 500);
//System.Windows.Forms.TextBox t = new System.Windows.Forms.TextBox();
//texturesForm.Controls.Add(t);
//texturesForm.ShowDialog();