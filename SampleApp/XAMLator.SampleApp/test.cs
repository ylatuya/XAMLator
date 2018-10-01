using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System;

namespace XAMLator.SampleApp
{
	public partial class Calculator2 : Xamarin.Forms.ContentPage
	{
		Xamarin.Forms.Label resultText;
		void InitializeComponent()
		{
			default(Xamarin.Forms.Button).Clicked += OnSelectNumber;
			default(Xamarin.Forms.Button).Clicked += OnSelectNumber;
			default(Xamarin.Forms.Button).Clicked += OnSelectNumber;
			default(Xamarin.Forms.Button).Clicked += OnSelectNumber;
			default(Xamarin.Forms.Button).Clicked += OnSelectNumber;
			default(Xamarin.Forms.Button).Clicked += OnSelectNumber;
			default(Xamarin.Forms.Button).Clicked += OnSelectNumber;
			default(Xamarin.Forms.Button).Clicked += OnSelectNumber;
			default(Xamarin.Forms.Button).Clicked += OnSelectNumber;
			default(Xamarin.Forms.Button).Clicked += OnSelectNumber;
			default(Xamarin.Forms.Button).Clicked += OnSelectOperator;
			default(Xamarin.Forms.Button).Clicked += OnSelectOperator;
			default(Xamarin.Forms.Button).Clicked += OnSelectOperator;
			default(Xamarin.Forms.Button).Clicked += OnSelectOperator;
			default(Xamarin.Forms.Button).Clicked += OnClear;
			default(Xamarin.Forms.Button).Clicked += OnCalculate;
		}
	}

	public partial class Calculator2 : ContentPage
	{
		public Calculator2()
		{
			InitializeComponent();
		}

		void OnSelectNumber(object sender, EventArgs args)
		{

		}

		void OnSelectOperator(object sender, EventArgs args)
		{

		}

		void OnClear(object sender, EventArgs args)
		{

		}

		void OnCalculate(object sender, EventArgs args)
		{

		}
	}

}