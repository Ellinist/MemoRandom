using System;
using MemoRandom.Models.Models;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using NLog;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Events;
using System.Windows;
using System.Windows.Threading;
using MemoRandom.Data.Controllers;
using MemoRandom.Data.Implementations;
using MemoRandom.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MemoRandom.Client.ViewModels
{
    public class ReasonsViewModel : BindableBase
    {
        #region PRIVATE FIELDS
        private string _reasonsViewTitle = "Справочник причин смерти";
        private readonly ILogger _logger; // Экземпляр журнала
        private readonly IEventAggregator _eventAggregator;
        private readonly IReasonsController _dbController;
        private bool _cancelButtonEnabled = false; // Для кнопки отмены
        private bool _deleteButtonEnabled = false; // Для кнопки удаления
        private bool _changeSaveButtonEnabled = false; // Для кнопки изменения/сохранения
        private bool _addSaveButtonEnabled = true;  // Для кнопки добавления/сохранения
        private bool _fieldsEnabled = false; // Для доступности полей окна
        private bool _changeMode = false; // Флаг редактирования записи
        private bool _addMode = false; // Флаг добавления записи
        private const string ChangeButton = "Изменить";
        private const string AddButton = "Добавить";
        private const string SaveButton = "Сохранить";
        private string _saveButtonText = ChangeButton;
        private string _addButtonText = AddButton;
        private string _changeButtonText = "Изменить";

        private string _reasonName;
        private string _reasonComment;
        private string _reasonDescription;
        private Reason _selectedReason;
        private ObservableCollection<Reason> _reasonsList = new();
        #endregion

        #region PROPS
        /// <summary>
        /// Заголовок окна справочника причин смерти
        /// </summary>
        public string ReasonsViewTitle
        {
            get => _reasonsViewTitle;
            set
            {
                _reasonsViewTitle = value;
                RaisePropertyChanged();
            }
        }
        /// <summary>
        /// Свойство доступности кнопки отмены
        /// </summary>
        public bool CancelButtonEnabled
        {
            get => _cancelButtonEnabled;
            set
            {
                _cancelButtonEnabled = value;
                RaisePropertyChanged(nameof(CancelButtonEnabled));
            }
        }
        /// <summary>
        /// Свойство доступности кнопки удаления
        /// </summary>
        public bool DeleteButtonEnabled
        {
            get => _deleteButtonEnabled;
            set
            {
                _deleteButtonEnabled = value;
                RaisePropertyChanged(nameof(DeleteButtonEnabled));
            }
        }
        /// <summary>
        /// Свойство доступности кнопки изменения/сохранения
        /// </summary>
        public bool ChangeSaveButtonEnabled
        {
            get => _changeSaveButtonEnabled;
            set
            {
                _changeSaveButtonEnabled = value;
                RaisePropertyChanged(nameof(ChangeSaveButtonEnabled));
            }
        }
        /// <summary>
        /// Свойство доступности кнопки добавления/сохранения
        /// </summary>
        public bool AddSaveButtonEnabled
        {
            get => _addSaveButtonEnabled;
            set
            {
                _addSaveButtonEnabled = value;
                RaisePropertyChanged(nameof(AddSaveButtonEnabled));
            }
        }
        /// <summary>
        /// Текст кнопки изменения/сохранения
        /// </summary>
        public string SaveButtonText
        {
            get => _saveButtonText;
            set
            {
                _saveButtonText = value;
                RaisePropertyChanged(nameof(SaveButtonText));
            }
        }
        /// <summary>
        /// Текст кнопки добавления/сохранения
        /// </summary>
        public string AddButtonText
        {
            get => _addButtonText;
            set
            {
                _addButtonText = value;
                RaisePropertyChanged(nameof(AddButtonText));
            }
        }
        /// <summary>
        /// Свойство доступности полей для редактирования
        /// </summary>
        public bool FieldsEnabled
        {
            get => _fieldsEnabled;
            set
            {
                _fieldsEnabled = value;
                RaisePropertyChanged(nameof(FieldsEnabled));
            }
        }
        /// <summary>
        /// Название причины смерти - является именем узла в иерархическом дереве
        /// </summary>
        public string ReasonName
        {
            get => _reasonName;
            set
            {
                _reasonName = value;
                RaisePropertyChanged(nameof(ReasonName));
            }
        }
        /// <summary>
        /// Комментарий к причине смерти
        /// </summary>
        public string ReasonComment
        {
            get => _reasonComment;
            set
            {
                if (_reasonComment == value) return;
                _reasonComment = value;
                RaisePropertyChanged(nameof(ReasonComment));
            }
        }
        /// <summary>
        /// Описание причины смерти
        /// </summary>
        public string ReasonDescription
        {
            get => _reasonDescription;
            set
            {
                if (_reasonDescription == value) return;
                _reasonDescription = value;
                RaisePropertyChanged(nameof(ReasonDescription));
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
        /// <summary>
        /// Коллекция причин смерти - для древовидного отображения
        /// </summary>
        public ObservableCollection<Reason> ReasonsList
        {
            get => _reasonsList;
            set
            {
                if (_reasonsList == value) return;
                _reasonsList = value;
            }
        }
        #endregion

        #region COMMANDS
        /// <summary>
        /// Команда загрузки окна справочника причин смерти
        /// </summary>
        public DelegateCommand OnLoadedReasonsViewCommand { get; private set; }
        /// <summary>
        ///Команда внесения причины смерти в иерархическое дерево
        /// </summary>
        public DelegateCommand<object> InsertCommand { get; private set; }
        /// <summary>
        /// Команда изменения выбранной причины смерти в иерархическом дереве
        /// </summary>
        public DelegateCommand ChangeCommand { get; private set; }
        /// <summary>
        /// Команда выбора элемента в иерархическом дереве причин смерти
        /// </summary>
        public DelegateCommand<object> SelectNodeCommand { get; private set; }
        /// <summary>
        /// Команда удаления узла (причины смерти)
        /// </summary>
        public DelegateCommand<object> DeleteNodeCommand { get; private set; }
        /// <summary>
        /// Команда отмены внесенных изменений
        /// </summary>
        public DelegateCommand<object> CancelCommand { get; private set; }
        /// <summary>
        /// Команда клика на пустом месте TreeView (отмена выбора узла)
        /// </summary>
        public DelegateCommand EmptyClickCommand { get; private set; }
        /// <summary>
        /// Команда записи (обновления) данных
        /// </summary>
        public DelegateCommand UpdateCommand { get; private set; }
        /// <summary>
        /// Команда получения данных
        /// </summary>
        public DelegateCommand LoadCommand { get; private set; }
        #endregion


        #region Блок отработки команд
        /// <summary>
        /// Загрузка окна и получение справочника причин смерти
        /// </summary>
        private void OnLoadedReasonsView()
        {
            Task.Factory.StartNew(() =>
            {
                var result = _dbController.GetReasonsList();
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    ReasonsList = result;
                    RaisePropertyChanged(nameof(ReasonsList));
                });
            });
            //ReasonsList = _dbController.GetReasonsList();
            //RaisePropertyChanged(nameof(ReasonsList));
        }
        /// <summary>
        /// Команда добавления записи в справочник причин смерти
        /// </summary>
        private void OnInsertCommand(object obj)
        {
            if (!_addMode) // Заход в блок внесения изменений в поля окна
            {
                SelectedReason = obj as Reason;
                CancelButtonEnabled = true; // Кнопка отмены разблокирована - на случай, если передумали
                ChangeSaveButtonEnabled = false; // В режиме добавления кнопка редактирования недоступна
                DeleteButtonEnabled = false; // В режиме редактирования кнопка удаления недоступна
                AddButtonText = SaveButton;

                FieldsEnabled = true;
                _addMode = true;
            }
            else // Занесение изменений в иерархическое дерево
            {
                if (ReasonName == null || ReasonName == string.Empty)
                {
                    MessageBox.Show("Нельзя создать причину смерти с пустым названием!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // Создаем новый экземпляр причины смерти
                Reason rsn = new()
                {
                    ReasonId = Guid.NewGuid(), // Регистрация нового идентификатора
                    ReasonName = ReasonName,
                    ReasonComment = ReasonComment ?? string.Empty,
                    ReasonDescription = ReasonDescription ?? string.Empty
                };
                if (SelectedReason != null) // Если узел выбран, то создаем дочку
                {
                    rsn.ReasonParentId = SelectedReason.ReasonId;
                    rsn.ReasonParent = SelectedReason;
                    SelectedReason.ReasonChildren.Add(rsn);
                    SelectedReason = rsn;
                }
                else
                {
                    rsn.ReasonParentId = Guid.Empty;
                    SelectedReason = rsn;
                    ReasonsList.Add(rsn); // Если узел не выбран, то создаем в корне
                }

                Task.Factory.StartNew(() =>
                {
                    var result = _dbController.AddReasonToList(rsn);
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        if (!result)
                        {
                            MessageBox.Show("Не удалось сохранить причину", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                        FieldsEnabled = false; // Делаем поля недоступными - пока не выберем узел и не выберем команду
                        CancelButtonEnabled = false; // После добавления новой записи кнопка отмены недоступна
                        ChangeSaveButtonEnabled = false; // После добавления новой записи кнопка изменения недоступна до выбора узла
                        DeleteButtonEnabled = false; // После добавления новой записи кнопка удаления недоступна до выбора узла
                        AddButtonText = AddButton; // Возвращаем название кнопки

                        SelectedReason.IsSelected = true;
                        RaisePropertyChanged(nameof(SelectedReason));
                        _addMode = false;
                    });
                });
            }
        }
        /// <summary>
        /// Команда внесения в иерархическое дерево произведенных изменений
        /// </summary>
        private void OnChangeCommand()
        {
            if (_changeMode) // Если в режиме редактирования - завершение
            {
                SelectedReason.ReasonName = ReasonName;
                SelectedReason.ReasonComment = ReasonComment;
                SelectedReason.ReasonDescription = ReasonDescription;

                FieldsEnabled = false;
                SaveButtonText = ChangeButton;
                _changeMode = false; // Флаг, что не в режиме редактирования
                AddSaveButtonEnabled = true; // При выходе из режима редактирования кнопка добавления становится доступной
                ChangeSaveButtonEnabled = false; // После изменения записи кнопка изменения недоступна до выбора узла
                DeleteButtonEnabled = false; // После изменения записи кнопка удаления недоступна до выбора узла
                CancelButtonEnabled = false; // После изменения записи кнопка отмены недоступна до выбора узла

                Task.Factory.StartNew(() =>
                {
                    var result = _dbController.UpdateReasonInList(SelectedReason);
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        if (!result)
                        {
                            MessageBox.Show("Не удалось обновить данные!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                });
            }
            else // Вход в режим редактирования
            {
                AddSaveButtonEnabled = false; // При входе в режим редактирования кнопка добавления недоступна
                DeleteButtonEnabled = false; // При входе в режим редактирования кнопка удаления недоступна
                CancelButtonEnabled = true; // В режиме редактирования кнопка отмены доступна
                FieldsEnabled = true; // Поля становятся доступными
                SaveButtonText = SaveButton; // Меняем название кнопки
                _changeMode = true; // Флаг, что в режиме редактирования
            }
        }
        /// <summary>
        /// Команда выбора элемента в иерархическом дереве причин смерти
        /// </summary>
        /// <param name="obj"></param>
        private void OnSelectNodeCommand(object obj)
        {
            if (obj == null)
            {
                DeleteButtonEnabled = false;     // При нулевом узле удалять нечего - кнопка недоступна
                ChangeSaveButtonEnabled = false; // При нулевом узле нечего редактировать - кнопка недоступна
                SetEmptyFields(); // Очищаем поля окна
            }
            else
            {
                DeleteButtonEnabled = true;     // Узел выбран - кнопка удаления доступна
                ChangeSaveButtonEnabled = true; // Узел выбран - кнопка редактирования/сохранения доступна
                SelectedReason = obj as Reason;
                ReasonName = SelectedReason?.ReasonName;
                ReasonComment = SelectedReason?.ReasonComment;
                ReasonDescription = SelectedReason?.ReasonDescription;
            }
        }
        /// <summary>
        /// Команда удаления узла (причины смерти)
        /// При этом автоматом удаляются все дочерние узлы
        /// Подумать, не вставить ли туда предупреждение об этом
        /// </summary>
        /// <param name="obj"></param>
        private void OnDeleteNodeCommand(object obj)
        {
            if (obj is not Reason selectedNode) return;

            if (selectedNode.ReasonParent != null)
            {
                selectedNode.ReasonParent.ReasonChildren.Remove(selectedNode.ReasonParent.ReasonChildren.First(x => x.ReasonId == selectedNode.ReasonId));
            }
            else
            {
                ReasonsList.Remove(selectedNode);
            }

            Task.Factory.StartNew(() =>
            {
                _dbController.DeleteReasonInList(selectedNode);
            });

            SelectedReason.IsSelected = false;
            SetEmptyFields(); // Очищаем поля окна
            DeleteButtonEnabled = false; // Удаление выполнено - кнопка удаления становится недоступной
            ChangeSaveButtonEnabled = false; // Удаление выполнено - кнопка редактирования становится недоступной
        }
        /// <summary>
        /// Команда отмены внесенных для узла изменений
        /// </summary>
        /// <param name="obj"></param>
        private void OnCancelCommand(object obj)
        {
            SelectedReason = obj as Reason;
            ReasonName = SelectedReason?.ReasonName;
            ReasonComment = SelectedReason?.ReasonComment;
            ReasonDescription = SelectedReason?.ReasonDescription;

            SelectedReason.IsSelected = true;
            CancelButtonEnabled = false; // После отмены кнопка становится недоступной
            AddSaveButtonEnabled = true; // После отмены кнопка добавления становится доступной
            ChangeSaveButtonEnabled = false; // После отмены кнопка изменения становится недоступной

            #region Все названия кнопок возвращаются в свои первоначальные состояния
            AddButtonText = AddButton;
            SaveButtonText = ChangeButton;
            #endregion

            FieldsEnabled = false;
            _addMode = false;
            _changeMode = false;
        }
        /// <summary>
        /// Команда обработки снятия выделения узла при клике на пустом месте TreeView
        /// </summary>
        private void OnEmptyClickCommand()
        {
            CancelButtonEnabled = false;     // Кнопка отмены недоступна - нечего отменять
            ChangeSaveButtonEnabled = false; // Кнопка редактирования/сохранения недоступна - нечего редактировать

            if (SelectedReason == null) return;

            SelectedReason.IsSelected = false;
            SelectedReason = null;
        }
        /// <summary>
        /// Метод установки пустых полей окна
        /// </summary>
        private void SetEmptyFields()
        {
            ReasonName = string.Empty;
            ReasonComment = string.Empty;
            ReasonDescription = string.Empty;
        }
        #endregion





        /// <summary>
        /// Инициализация комманд
        /// </summary>
        private void InitializeCommands()
        {
            InsertCommand = new DelegateCommand<object>(OnInsertCommand);
            SelectNodeCommand = new DelegateCommand<object>(OnSelectNodeCommand);
            DeleteNodeCommand = new DelegateCommand<object>(OnDeleteNodeCommand);
            CancelCommand = new DelegateCommand<object>(OnCancelCommand);
            EmptyClickCommand = new DelegateCommand(OnEmptyClickCommand);
            ChangeCommand = new DelegateCommand(OnChangeCommand);
            OnLoadedReasonsViewCommand = new DelegateCommand(OnLoadedReasonsView);
        }

        #region CTOR

        public ReasonsViewModel(ILogger logger, IEventAggregator eventAggregator, IReasonsController dbController)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _dbController = dbController ?? throw new ArgumentNullException(nameof(dbController));

            InitializeCommands();
        }
        #endregion
    }
}
