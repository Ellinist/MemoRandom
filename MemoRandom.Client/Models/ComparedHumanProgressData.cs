using System;
using System.Windows.Media.Imaging;
using Prism.Mvvm;

namespace MemoRandom.Client.Models
{
    public class ComparedHumanProgressData : BindableBase
    {
        #region PRIVATE FIELDS
        private BitmapSource _previousImage;
        private BitmapSource _nextImage;

        private double _startValue;
        private double _stopValue;
        private double _currentValue;

        private string _previousHumanName;
        private string _previousHumanBirthDate;
        private string _previousHumanDeathDate;
        private string _previousHumanFullYears;
        private string _previousHumanOverLifeDate;
        private string _spentDaysFromPreviousHuman;

        private string _currentHumanFullName;
        private string _currentHumanBirthDate;
        private string _currentHumanLivedPeriod;

        private string _nextHumanName;
        private string _nextHumanBirthDate;
        private string _nextHumanDeathDate;
        private string _nextHumanFullYears;
        private string _nextHumanOverLifeDate;
        private string _restDaysToNextHuman;
        #endregion

        #region PROPS
        /// <summary>
        /// Изображение предыдущего актора
        /// </summary>
        public BitmapSource PreviousImage
        {
            get => _previousImage;
            set
            {
                _previousImage = value;
                RaisePropertyChanged(nameof(PreviousImage));
            }
        }
        /// <summary>
        /// Изображение следующего актора
        /// </summary>
        public BitmapSource NextImage
        {
            get => _nextImage;
            set
            {
                _nextImage = value;
                RaisePropertyChanged(nameof(NextImage));
            }
        }

        /// <summary>
        /// Начальное значение прогресс-индикатора
        /// </summary>
        public double StartValue
        {
            get => _startValue;
            set
            {
                _startValue = value;
                RaisePropertyChanged(nameof(StartValue));
            }
        }
        /// <summary>
        /// Конечное значение прогресс-индикатора
        /// </summary>
        public double StopValue
        {
            get => _stopValue;
            set
            {
                _stopValue = value;
                RaisePropertyChanged(nameof(StopValue));
            }
        }
        /// <summary>
        /// Текущее значение прогресс-индикатора
        /// </summary>
        public double CurrentValue
        {
            get => _currentValue;
            set
            {
                _currentValue = value;
                RaisePropertyChanged(nameof(CurrentValue));
            }
        }

        /// <summary>
        /// Имя предыдущего актора
        /// </summary>
        public string PreviousHumanName
        {
            get => _previousHumanName;
            set
            {
                _previousHumanName = value;
                RaisePropertyChanged(nameof(PreviousHumanName));
            }
        }
        /// <summary>
        /// Дата рождения предыдущего актора
        /// </summary>
        public string PreviousHumanBirthDate
        {
            get => _previousHumanBirthDate;
            set
            {
                _previousHumanBirthDate = value;
                RaisePropertyChanged(nameof(PreviousHumanBirthDate));
            }
        }
        /// <summary>
        /// Дата смерти предыдущего актора
        /// </summary>
        public string PreviousHumanDeathDate
        {
            get => _previousHumanDeathDate;
            set
            {
                _previousHumanDeathDate = value;
                RaisePropertyChanged(nameof(PreviousHumanDeathDate));
            }
        }
        /// <summary>
        /// Число прожитых лет у предыдущего актора
        /// </summary>
        public string PreviousHumanFullYears
        {
            get => _previousHumanFullYears;
            set
            {
                _previousHumanFullYears = value;
                RaisePropertyChanged(nameof(PreviousHumanFullYears));
            }
        }
        /// <summary>
        /// Дата, когда предыдущий актор был пережит
        /// </summary>
        public string PreviousHumanOverLifeDate
        {
            get => _previousHumanOverLifeDate;
            set
            {
                _previousHumanOverLifeDate = value;
                RaisePropertyChanged(nameof(PreviousHumanOverLifeDate));
            }
        }
        /// <summary>
        /// Сколько прошло после преодоления предыдущего актора
        /// </summary>
        public string SpentDaysFromPreviousHuman
        {
            get => _spentDaysFromPreviousHuman;
            set
            {
                _spentDaysFromPreviousHuman = value;
                RaisePropertyChanged(nameof(SpentDaysFromPreviousHuman));
            }
        }

        /// <summary>
        /// Имя текущего анализируемого человека
        /// </summary>
        public string CurrentHumanFullName
        {
            get => _currentHumanFullName;
            set
            {
                _currentHumanFullName = value;
                RaisePropertyChanged(nameof(CurrentHumanFullName));
            }
        }
        /// <summary>
        /// Дата рождения текущего анализируемого человека
        /// </summary>
        public string CurrentHumanBirthDate
        {
            get => _currentHumanBirthDate;
            set
            {
                _currentHumanBirthDate = value;
                RaisePropertyChanged(nameof(CurrentHumanBirthDate));
            }
        }
        /// <summary>
        /// Количество прожитых лет текущим анализируемым человеком
        /// </summary>
        public string CurrentHumanLivedPeriod
        {
            get => _currentHumanLivedPeriod;
            set
            {
                _currentHumanLivedPeriod = value;
                RaisePropertyChanged(nameof(CurrentHumanLivedPeriod));
            }
        }

        /// <summary>
        /// Имя следующего актора
        /// </summary>
        public string NextHumanName
        {
            get => _nextHumanName;
            set
            {
                _nextHumanName = value;
                RaisePropertyChanged(nameof(NextHumanName));
            }
        }
        /// <summary>
        /// Дата рождения следующего актора
        /// </summary>
        public string NextHumanBirthDate
        {
            get => _nextHumanBirthDate;
            set
            {
                _nextHumanBirthDate = value;
                RaisePropertyChanged(nameof(NextHumanBirthDate));
            }
        }
        /// <summary>
        /// Дата смерти следующего актора
        /// </summary>
        public string NextHumanDeathDate
        {
            get => _nextHumanDeathDate;
            set
            {
                _nextHumanDeathDate = value;
                RaisePropertyChanged(nameof(NextHumanDeathDate));
            }
        }
        /// <summary>
        /// Число прожитых лет у следующего актора
        /// </summary>
        public string NextHumanFullYears
        {
            get => _nextHumanFullYears;
            set
            {
                _nextHumanFullYears = value;
                RaisePropertyChanged(nameof(NextHumanFullYears));
            }
        }
        /// <summary>
        /// Дата, когда будет пережит следующий актор
        /// </summary>
        public string NextHumanOverLifeDate
        {
            get => _nextHumanOverLifeDate;
            set
            {
                _nextHumanOverLifeDate = value;
                RaisePropertyChanged(nameof(NextHumanOverLifeDate));
            }
        }
        /// <summary>
        /// Сколько осталось до пережития следующего актора
        /// </summary>
        public string RestDaysToNextHuman
        {
            get => _restDaysToNextHuman;
            set
            {
                _restDaysToNextHuman = value;
                RaisePropertyChanged(nameof(RestDaysToNextHuman));
            }
        }
        #endregion
    }
}
