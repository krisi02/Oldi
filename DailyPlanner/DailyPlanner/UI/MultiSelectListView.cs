using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace DailyPlanner.UI
{
	public class MultiSelectListView : ListView
	{
		public MultiSelectListView()
			: base(ListViewCachingStrategy.RecycleElement)
		{
			ContextActions = new List<MenuItem>();
		}

		public event EventHandler<int> ItemLongTapped;
		public IList<MenuItem> ContextActions
		{
			get { return (IList<MenuItem>)GetValue(ContextActionsProperty); }
			set { SetValue(ContextActionsProperty, value); }
		}

		public static readonly BindableProperty ContextActionsProperty = BindableProperty.Create(
			propertyName: "ContextActions",
			returnType: typeof(IList<MenuItem>),
			declaringType: typeof(MultiSelectListView),
			defaultValue: null);

		public static readonly BindableProperty IsMultiSelectionEnabledProperty = BindableProperty.Create(
			propertyName: "IsMultiSelectionEnabled",
			returnType: typeof(bool),
			declaringType: typeof(MultiSelectListView),
			defaultBindingMode: BindingMode.TwoWay,
			defaultValue: false);

		public bool IsMultiSelectionEnabled
		{
			get { return (bool)GetValue(IsMultiSelectionEnabledProperty); }
			set { SetValue(IsMultiSelectionEnabledProperty, value); }
		}

		public static readonly BindableProperty ItemTapCommandProperty = BindableProperty.Create(
			propertyName: "ItemTapCommand",
			returnType: typeof(ICommand),
			declaringType: typeof(MultiSelectListView));

		public ICommand ItemTapCommand
		{
			get { return (ICommand)GetValue(ItemTapCommandProperty); }
			set { SetValue(ItemTapCommandProperty, value); }
		}

		public static readonly BindableProperty ItemLongTapCommandProperty = BindableProperty.Create(
			propertyName: "ItemLongTapCommand",
			returnType: typeof(ICommand),
			declaringType: typeof(MultiSelectListView));

		public ICommand ItemLongTapCommand
		{
			get { return (ICommand)GetValue(ItemLongTapCommandProperty); }
			set { SetValue(ItemLongTapCommandProperty, value); }
		}

		public void OnItemLongTapped(int itemIndex)
		{
			ItemLongTapped?.Invoke(this, itemIndex);
		}
	}
}
