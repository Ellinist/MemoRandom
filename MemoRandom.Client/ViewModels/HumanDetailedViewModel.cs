using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MemoRandom.Data.Interfaces;
using MemoRandom.Models.Models;
using Prism.Commands;
using Prism.Mvvm;
//using System.Drawing;

namespace MemoRandom.Client.ViewModels
{
    /// <summary>
    /// Класс модели представления формирования нового человека
    /// </summary>
    public class HumanDetailedViewModel : BindableBase
    {
        public Action CloseAction { get; set; }

        #region PRIVATE FIELDS
        private readonly IHumansController _humanController;

        private string   _lastName;
        private string   _firstName;
        private string   _patronymic;
        private DateTime _birthDate;
        private string   _birthCountry;
        private string   _birthPlace;
        private DateTime _deathDate;
        private string   _deathCountry;
        private string   _deathPlace;
        private Guid     _deathReasonId;
        private Reason   _selectedReason;
        private BitmapSource _targetImageSource;
        private BitmapSource _sourceImageSource;
        private string _humanComments;
        private int _daysLived;
        private double _fullYearsLived;
        private ObservableCollection<Reason> _reasonsList;
        private List<Reason> _plainReasonsList;
        private string _humanDeathReasonName;
        private bool _openComboState = false; // По умолчанию комбобокс свернут
        private double _left = 0; // Левый верхний угол изображения на канве (координата X)
        private double _top = 0;  // Левый верхний угол изображения на канве (координата Y)
        private double _scaleX = 1; // Масштаб по оси X
        private double _scaleY = 1; // Масштаб по оси Y

        private double _startX;
        private double _startY;
        private double _currentX;
        private double _currentY;
        private double _deltaX;
        private double _deltaY;
        private double _imageX;
        private double _imageY;
        private Canvas _canvas;
        private Window _window;
        #endregion

        #region PROPS
        /// <summary>
        /// Фамилия человека
        /// </summary>
        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Имя человека
        /// </summary>
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Отчество человека
        /// </summary>
        public string Patronymic
        {
            get => _patronymic;
            set
            {
                _patronymic = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Дата рождения
        /// </summary>
        public DateTime BirthDate
        {
            get => _birthDate;
            set
            {
                _birthDate = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Страна рождения
        /// </summary>
        public string BirthCountry
        {
            get => _birthCountry;
            set
            {
                _birthCountry = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Место рождения
        /// </summary>
        public string BirthPlace
        {
            get => _birthPlace;
            set
            {
                _birthPlace = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Дата смерти
        /// </summary>
        public DateTime DeathDate
        {
            get => _deathDate;
            set
            {
                _deathDate = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Страна смерти
        /// </summary>
        public string DeathCountry
        {
            get => _deathCountry;
            set
            {
                _deathCountry = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Место смерти
        /// </summary>
        public string DeathPlace
        {
            get => _deathPlace;
            set
            {
                _deathPlace = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Идентификатор причины смерти
        /// </summary>
        public Guid DeathReasonId
        {
            get => _deathReasonId;
            set
            {
                _deathReasonId = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Свойство - итоговое изображение
        /// </summary>
        public BitmapSource TargetImageSource
        {
            get => _targetImageSource;
            set
            {
                _targetImageSource = value;
                RaisePropertyChanged(nameof(TargetImageSource));
            }
        }

        /// <summary>
        /// Свойство - исходное изображение
        /// </summary>
        public BitmapSource SourceImageSource
        {
            get => _sourceImageSource;
            set
            {
                _sourceImageSource = value;
                RaisePropertyChanged(nameof(SourceImageSource));
            }
        }

        /// <summary>
        /// Расширенный комментарий
        /// </summary>
        public string HumanComments
        {
            get => _humanComments;
            set
            {
                _humanComments = value;
                RaisePropertyChanged(nameof(HumanComments));
            }
        }

        /// <summary>
        /// Число полных прожитых дней
        /// </summary>
        public int DaysLived
        {
            get => _daysLived;
            set
            {
                _daysLived = value;
                RaisePropertyChanged(nameof(DaysLived));
            }
        }

        /// <summary>
        /// Число полных прожитых лет
        /// </summary>
        public double FullYearsLived
        {
            get => _fullYearsLived;
            set
            {
                _fullYearsLived = value;
                RaisePropertyChanged(nameof(FullYearsLived));
            }
        }

        /// <summary>
        /// Иерархическая коллекция справочника причин смерти
        /// </summary>
        public ObservableCollection<Reason> ReasonsList
        {
            get => _reasonsList;
            set
            {
                _reasonsList = value;
                RaisePropertyChanged(nameof(ReasonsList));
            }
        }

        /// <summary>
        /// Плоский список справочника причин смерти для вытягивания названия причины
        /// </summary>
        public List<Reason> PlainReasonsList
        {
            get => _plainReasonsList;
            set
            {
                _plainReasonsList = value;
                RaisePropertyChanged(nameof(PlainReasonsList));
            }
        }

        /// <summary>
        /// Отображаемая причина смерти
        /// </summary>
        public string HumanDeathReasonName
        {
            get => _humanDeathReasonName;
            set
            {
                _humanDeathReasonName = value;
                RaisePropertyChanged(nameof(HumanDeathReasonName));
            }
        }

        /// <summary>
        /// Состояние выпадающего окна с TreeView - развернуто или свернуто
        /// </summary>
        public bool OpenComboState
        {
            get => _openComboState;
            set
            {
                _openComboState = value;
                RaisePropertyChanged(nameof(OpenComboState));
            }
        }

        /// <summary>
        /// Выбранный узел (причина смерти) в иерархическом дереве
        /// </summary>
        public Reason SelectedReason
        {
            get => _selectedReason;
            set
            {
                if (_selectedReason == value) return;
                _selectedReason = value;
                RaisePropertyChanged(nameof(SelectedReason));
            }
        }

        public double Left
        {
            get => _left;
            set
            {
                _left = value;
                RaisePropertyChanged(nameof(Left));
            }
        }

        public double Top
        {
            get => _top;
            set
            {
                _top = value;
                RaisePropertyChanged(nameof(Top));
            }
        }

        public double ScaleX
        {
            get => _scaleX;
            set
            {
                _scaleX = value;
                RaisePropertyChanged(nameof(ScaleX));
            }
        }

        public double ScaleY
        {
            get => _scaleY;
            set
            {
                _scaleY = value;
                RaisePropertyChanged(nameof(ScaleY));
            }
        }


        public double StartX
        {
            get => _startX;
            set { _startX = value; RaisePropertyChanged(nameof(StartX)); }
        }

        public double StartY
        {
            get => _startY;
            set { _startY = value; RaisePropertyChanged(nameof(StartY)); }
        }

        public double CurrentX
        {
            get => _currentX;
            set { _currentX = value; RaisePropertyChanged(nameof(CurrentX)); }
        }

        public double CurrentY
        {
            get => _currentY;
            set { _currentY = value; RaisePropertyChanged(nameof(CurrentY)); }
        }

        public double DeltaX
        {
            get => _deltaX;
            set { _deltaX = value; RaisePropertyChanged(nameof(DeltaX)); }
        }

        public double DeltaY
        {
            get => _deltaY;
            set { _deltaY = value; RaisePropertyChanged(nameof(DeltaY)); }
        }

        public double ImageX
        {
            get => _imageX;
            set { _imageX = value; RaisePropertyChanged(nameof(ImageX)); }
        }

        public double ImageY
        {
            get => _imageY;
            set { _imageY = value; RaisePropertyChanged(nameof(ImageY)); }
        }

        public Canvas Canv
        {
            get => _canvas;
            set
            {
                _canvas = value;
                RaisePropertyChanged(nameof(Canv));
            }
        }

        public Window DetailedWiew
        {
            get => _window;
            set
            {
                _window = value;
                RaisePropertyChanged(nameof(DetailedWiew));
            }
        }
        #endregion

        private bool _isDown = false;

        #region Блок работы с изображением
        private void SetTargetImage(object obj)
        {
            var Canv = obj as Canvas;
            if (Canv != null)
            {
                var target = new RenderTargetBitmap((int)(Canv.RenderSize.Width), (int)(Canv.RenderSize.Height), 96, 96, PixelFormats.Pbgra32);
                var brush = new VisualBrush(Canv);

                var visual = new DrawingVisual();
                var drawingContext = visual.RenderOpen();


                drawingContext.DrawRectangle(brush, null, new Rect(new Point(0, 0), new Point(Canv.RenderSize.Width, Canv.RenderSize.Height)));

                drawingContext.Close();

                target.Render(visual);

                TargetImageSource = target;
            }
        }

        /// <summary>
        /// Обработчик нажатия левой кнопки мыши на исходном изображении
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PersonImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var obj = (sender as Image);
            if (!_isDown)
            {
                _isDown = true;
                if (obj != null)
                {
                    Point t2 = obj.PointToScreen(Mouse.GetPosition(obj));
                    StartX = t2.X;
                    StartY = t2.Y;
                }
            }
        }

        /// <summary>
        /// Обработчик отпускания левой кнопки мыши на исходном изображении
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PersonImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDown = false;
        }

        /// <summary>
        /// Обработчик движения мыши над исходным изображением
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PersonImage_MouseMove(object sender, MouseEventArgs e)
        {
            var obj = (sender as Image);
            if (_isDown)
            {
                Point t = obj.PointToScreen(Mouse.GetPosition(obj));

                CurrentX = t.X;
                CurrentY = t.Y;

                DeltaX = CurrentX - StartX;
                DeltaY = CurrentY - StartY;

                Left += DeltaX;
                Top += DeltaY;

                StartX = CurrentX;
                StartY = CurrentY;
            }
        }

        /// <summary>
        /// Обработчик прокрутки колесом мыши над исходным изображением
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PersonImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
            {
                ScaleX -= 0.01;
                ScaleY -= 0.01;
            }
            else
            {
                ScaleX += 0.01;
                ScaleY += 0.01;
            }
        }
        #endregion

        #region COMMANDS
        /// <summary>
        /// Команда сохранения данных по человеку
        /// </summary>
        public DelegateCommand SaveHumanCommand { get; private set; }
        
        /// <summary>
        /// Команда загрузки изображения
        /// </summary>
        public DelegateCommand ImageLoadCommand { get; private set; }

        public DelegateCommand<object> SelectNodeCommand { get; private set; }

        public DelegateCommand<object> SetTargetImageCommand { get; private set; }
        #endregion

        #region COMMANDS IMPLEMENTATION
        /// <summary>
        /// Загрузка окна добавления/редактирования человека
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void DetailedView_Loaded(object sender, RoutedEventArgs e/*, Canvas canvas*/)
        {
            DetailedWiew = sender as Window;

            Human human = _humanController.GetCurrentHuman();

            if (human != null)
            {
                LastName = human.LastName;
                FirstName = human.FirstName;
                Patronymic = human.Patronymic;
                BirthDate = human.BirthDate;
                BirthCountry = human.BirthCountry;
                BirthPlace = human.BirthPlace;
                DeathDate = human.DeathDate;
                DeathCountry = human.DeathCountry;
                DeathPlace = human.DeathPlace;
                HumanComments = human.HumanComments;
                DeathReasonId = human.DeathReasonId;
                TargetImageSource = (BitmapSource)_humanController.GetHumanImage(); // Загружаем изображение
            }
            else
            {
                LastName = "Введите фамилию";
                FirstName = "Введите имя";
                Patronymic = "Введите отчество";
                BirthDate = DateTime.Now;
                BirthCountry = "Введите страну рождения";
                BirthPlace = "Введите место рождения";
                DeathDate = DateTime.Now;
                DeathCountry = "Введите страну смерти";
                DeathPlace = "Введите место смерти";
                HumanComments = "Введите краткое описание";
            }
            ReasonsList = Reasons.ReasonsCollection;
            PlainReasonsList = Reasons.PlainReasonsList;
            var t = PlainReasonsList.Find(x => x.ReasonId == DeathReasonId);
            if (t != null)
            {
                HumanDeathReasonName = t.ReasonName;
            }
            RaisePropertyChanged(nameof(ReasonsList));
            RaisePropertyChanged(nameof(HumanDeathReasonName));

            //var positionTransform = canvas.TransformToAncestor(sender as Window);
            //var areaPosition = positionTransform.Transform(new Point(0, 0));
            //ImageX = areaPosition.X;
            //ImageY = areaPosition.Y;
        }

        public void SourceCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            var canvPoint = (sender as Canvas).PointToScreen(new Point(0, 0));

            ImageX = canvPoint.X;
            ImageY = canvPoint.Y;
            //var positionTransform = (sender as Canvas).TransformToAncestor(DetailedWiew);
            //var areaPosition = positionTransform.Transform(new Point(0, 0));
            //ImageX = areaPosition.X;
            //ImageY = areaPosition.Y;
        }

        //public void DetailesView_LocationChanged(object sender, EventArgs e, Canvas canvas)
        //{
        //    var positionTransform = canvas.TransformToAncestor(sender as Window);
        //    var areaPosition = positionTransform.Transform(new Point(0, 0));
        //    ImageX = areaPosition.X;
        //    ImageY = areaPosition.Y;
        //}

        /// <summary>
        /// Сохранение данных по человеку
        /// </summary>
        private void SaveHuman()
        {
            var curHuman = _humanController.GetCurrentHuman();
            if (curHuman != null) // Редактирование
            {
                curHuman.LastName = LastName;
                curHuman.FirstName = FirstName;
                curHuman.Patronymic = Patronymic;
                curHuman.BirthDate = BirthDate;
                curHuman.BirthCountry = BirthCountry;
                curHuman.BirthPlace = BirthPlace;
                curHuman.DeathDate = DeathDate;
                curHuman.DeathCountry = DeathCountry;
                curHuman.DeathPlace = DeathPlace;
                curHuman.ImageFile = TargetImageSource != null ? curHuman.HumanId.ToString() + ".jpg" : string.Empty;
                curHuman.HumanComments = HumanComments;
                curHuman.DeathReasonId = DeathReasonId;
                curHuman.DaysLived = (DeathDate - BirthDate).Days; // Считаем число прожитых дней
                curHuman.FullYearsLived = (float)((DeathDate - BirthDate).Days / 365.25D); // Считаем число полных прожитых лет

                _humanController.SetCurrentHuman(curHuman);
            }
            else // Добавление нового
            {
                var newHumanId = Guid.NewGuid();
                Human human = new()
                {
                    HumanId = newHumanId,
                    LastName = LastName,
                    FirstName = FirstName,
                    Patronymic = Patronymic,
                    BirthDate = BirthDate,
                    BirthCountry = BirthCountry,
                    BirthPlace = BirthPlace,
                    DeathDate = DeathDate,
                    DeathCountry = DeathCountry,
                    DeathPlace = DeathPlace,
                    ImageFile = TargetImageSource != null ? newHumanId.ToString() + ".jpg" : string.Empty,
                    HumanComments = HumanComments,
                    DeathReasonId = DeathReasonId,
                    DaysLived = (DeathDate - BirthDate).Days, // Считаем число прожитых дней
                    FullYearsLived = (float)((DeathDate - BirthDate).Days / 365.25) // Считаем число полных прожитых лет
            };

                _humanController.SetCurrentHuman(human);
            }

            _humanController.UpdateHumans(BitmapSourceToBitmapImage(TargetImageSource));

            CloseAction(); // Закрываем окно
        }

        /// <summary>
        /// Преобразование экранного изображения в BitmapImage
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        private BitmapImage BitmapSourceToBitmapImage(BitmapSource src)
        {
            if (src == null) return null;

            MemoryStream ms = new MemoryStream();
            BmpBitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(src));
            encoder.Save(ms);
            ms.Position = 0;

            BitmapImage myBitmapImage = new BitmapImage();
            myBitmapImage.BeginInit();
            myBitmapImage.StreamSource = ms;
            myBitmapImage.EndInit();
            return myBitmapImage;
        }

        private void SelectNode(object obj)
        {
            SelectedReason = obj as Reason;
            if(SelectedReason != null)
            {
                DeathReasonId = SelectedReason.ReasonId;
                HumanDeathReasonName = SelectedReason.ReasonName;
                RaisePropertyChanged(nameof(HumanDeathReasonName));
                OpenComboState = false;
                RaisePropertyChanged(nameof(OpenComboState));
            }
        }

        /// <summary>
        /// Метод загрузки изображения из буфера обмена
        /// </summary>
        private void ImageLoad()
        {
            if (Clipboard.ContainsImage())
            {
                SourceImageSource = Clipboard.GetImage();
                var w = SourceImageSource.Width;
                var h = SourceImageSource.Height;
                Left = -(h - 350) / 2;
                Top = -(w - 450) / 2;
            }
            else
            {
                MessageBox.Show("Not an image!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        /// <summary>
        /// Инициализация команд
        /// </summary>
        private void InitializeCommands()
        {
            SaveHumanCommand  = new DelegateCommand(SaveHuman);
            ImageLoadCommand  = new DelegateCommand(ImageLoad);
            SelectNodeCommand = new DelegateCommand<object>(SelectNode);
            SetTargetImageCommand = new DelegateCommand<object>(SetTargetImage);
        }

        #region CTOR
        public HumanDetailedViewModel(IHumansController humansController)
        {
            _humanController = humansController ?? throw new ArgumentNullException(nameof(humansController));

            InitializeCommands();
        }
        #endregion
    }
}



///// <summary>
///// Преобразование байтового массива в BitmapImage
///// </summary>
///// <param name="array"></param>
///// <returns></returns>
//private BitmapImage ConvertFromByteArray(byte[] array)
//{
//    if (array == null) return null;

//    BitmapImage myBitmapImage = new BitmapImage();
//    myBitmapImage.BeginInit();
//    myBitmapImage.StreamSource = new MemoryStream(array);
//    myBitmapImage.DecodePixelWidth = 200;
//    myBitmapImage.EndInit();
//    return myBitmapImage;
//}

///// <summary>
///// Преобразование BitmapSource в массив байтов
///// </summary>
///// <param name="src"></param>
///// <returns></returns>
//private byte[] ConvertFromBitmapSource(BitmapSource src)
//{
//    if(src == null) return null;

//    byte[] bit;
//    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
//    encoder.QualityLevel = 100;
//    using (MemoryStream stream = new MemoryStream())
//    {
//        encoder.Frames.Add(BitmapFrame.Create(src));
//        encoder.Save(stream);
//        bit = stream.ToArray();
//        stream.Close();
//    }

//    return bit;
//}
