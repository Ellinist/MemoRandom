using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Prism.Mvvm;

namespace MemoRandom.Client.Common.Models
{
    /// <summary>
    /// Основной класс причины смерти
    /// </summary>
    public class Reason : BindableBase
    {
        #region PRIVATE FIELDS
        private Guid _reasonId;
        private string _reasonName;
        private string _reasonComment;
        private string _reasonDescription;
        private ObservableCollection<Reason> _reasonChildren = new();
        private bool _isSelected;
        #endregion

        /// <summary>
        /// Guid идентификатор причины смерти
        /// </summary>
        [Key]
        public Guid ReasonId
        {
            get => _reasonId;
            set
            {
                if (_reasonId == value) return;
                _reasonId = value;
                RaisePropertyChanged(nameof(ReasonId));
            }
        }

        /// <summary>
        /// Название причины смерти
        /// </summary>
        public string ReasonName
        {
            get => _reasonName;
            set
            {
                if (_reasonName == value) return;
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
        /// Подробное описание причины смерти
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
        /// Навигационное свойство - родительская причина
        /// </summary>
        public virtual Reason ReasonParent { get; set; }

        /// <summary>
        /// Идентификатор родительской причины
        /// </summary>
        public Guid ReasonParentId { get; set; }

        /// <summary>
        /// Коллекция дочерних причин смерти
        /// </summary>
        public ObservableCollection<Reason> ReasonChildren
        {
            get => _reasonChildren;
            set
            {
                _reasonChildren = value;
                RaisePropertyChanged(nameof(ReasonChildren));
            }
        }

        /// <summary>
        /// Флаг выбора причины смерти в TreeView
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                RaisePropertyChanged(nameof(IsSelected));
            }
        }


        #region CTOR
        ///// <summary>
        ///// Конструктор
        ///// </summary>
        //public Reason()
        //{
        //    ReasonChildren = new ObservableCollection<Reason>();
        //}
        #endregion
    }
}
