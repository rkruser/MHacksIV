﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Text;
using System.Net.Http;
using Windows.Data.Xml.Dom;

namespace Dining_App.Data
{
    enum Halls { BURSLEY, EQUAD, HILL, MARKLEY, NQUAD, SQUAD, TWIGS };
    enum Meals { BREAKFAST, LUNCH, DINNER };



    class BigData
    {
        public static string[] hallNames = { "Bursley",
                                        "East Quad", 
                                        "Hill Dining Center", 
                                        "Markley", 
                                        "North Quad", 
                                        "South Quad", 
                                        "Twigs at Oxford" };

        public string getHallNames(int i)
        {
            return hallNames[i];
        }

        List<DiningHall> _diningHallList;
        public List<SearchHit> _searchResults;
        private int _curDate;

        // This function gets the menu from the url
        public async Task GetMenus()
        {
            for (int i = 0; i < this._diningHallList.Count(); ++i)
            {
                string url = this._createURL(this._curDate, i);
                string menuFromOnline = await new HttpClient().GetStringAsync(url);
                this._diningHallList[i].addMenu(this._curDate);
                this.parseMenu(this._diningHallList[i], menuFromOnline);
            }
        }

        private void parseMenu(DiningHall diningHall, string xmlMenu)
        {
            XElement XML = XElement.Parse(xmlMenu);

            XElement Menu = XML.Element("menu");
            XElement Breakfast = Menu.Element("meal");
            IEnumerable<XElement> BCourses = Breakfast.Elements("course");
            IEnumerable<XElement> Food = BCourses.Elements("menuitem");
            foreach (XElement food in Food)
            {
                XElement Name = food.Element("name");
                diningHall.addtoMenu(0, Name.Value);
            }

            XElement Lunch = (XElement)Breakfast.NextNode;
            IEnumerable<XElement> LCourses = Lunch.Elements("course");
            IEnumerable<XElement> LFood = LCourses.Elements("menuitem");
            foreach (XElement food in LFood)
            {
                XElement Name = food.Element("name");
                diningHall.addtoMenu(1, Name.Value);
            }

            XElement Dinner = (XElement)Lunch.NextNode;
            IEnumerable<XElement> DCourses = Dinner.Elements("course");
            IEnumerable<XElement> DFood = DCourses.Elements("menuitem");
            foreach (XElement food in DFood)
            {
                XElement Name = food.Element("name");
                diningHall.addtoMenu(2, Name.Value);
            }
        }

        public BigData()
        {
            this._diningHallList = new List<DiningHall>();
            this._searchResults = new List<SearchHit>();
            foreach (string name in BigData.hallNames)
            {
                var newDiningHall = new DiningHall(name);
                string url = _createURL(this._curDate, this._diningHallList.Count());
                this._diningHallList.Add(newDiningHall);
            }
        }

        public List<string> getDiningHallNames()
        {
            List<string> names = new List<string>();

            foreach (DiningHall hall in this._diningHallList)
            {
                names.Add(hall.name());
            }

            return names;
        }

        public void clearSearch()
        {
            _searchResults.Clear();
        }

        public void Search(int days, string FoodName)
        {
            for (int i = 0; i < days; ++i)
            {
                if (days > this._curDate)
                {
                    this.ChangeDate(); //update the time while we search the other stuff
                }

                for (int j = 0; j < this._diningHallList.Count(); ++j)
                {
                    if (this._diningHallList[j].look(i, FoodName))
                    {
                        this._searchResults.Add(new SearchHit(j, i));
                    }
                }
            }

        }

        public async void ChangeDate() //increment date by one and load menus
        {
            ++this._curDate;
            await this.GetMenus();

        }

        public Menu loadMenu(int day, int Hall)
        {
            return this._diningHallList[Hall].getMenu(day);

        }

        private string _createURL(int Day, int Name)
        {
            string location;
            string url;
            switch (Name)
            {
                case (int)Halls.BURSLEY:
                    location = "BURSLEY%20DINING%20HALL";
                    break;
                case (int)Halls.EQUAD:
                    location = "EAST%20QUAD%20DINING%20HALL";
                    break;
                case (int)Halls.HILL:
                    location = "HILL%20DINING%20CENTER";
                    break;
                case (int)Halls.MARKLEY:
                    location = "MARKLEY%20DINING%20HALL";
                    break;
                case (int)Halls.NQUAD:
                    location = "NORTH%20QUAD%20DINING%20HALL";
                    break;
                case (int)Halls.SQUAD:
                    location = "SOUTH%20QUAD%20DINING%20HALL";
                    break;
                case (int)Halls.TWIGS:
                    location = "TWIGS%20AT%20OXFORD";
                    break;
                default: //should never hit this 
                    location = "";
                    break;
            }
            if (Day == 0)
            {
                url = "http://www.housing.umich.edu/files/helper_files/js/menu2xml.php?location=" + location + "&date=today";
            }
            else
            {
                DateTime date = DateTime.Today.Date.AddDays(Day);

                url = "http://www.housing.umich.edu/files/helper_files/js/menu2xml.php?location=" + location + "&date=" + date.ToString("yyyy-MM-dd");
            }
            return url;
        }
    }

    class DiningHall
    {
        string _name;
        List<Menu> _menu; //indexed by days from today

        public DiningHall()
        {
            this._name = "";
            this._menu = new List<Menu>();
        }
        public DiningHall(string Name)
        {
            this._name = Name;
            this._menu = new List<Menu>();
        }

        public void addMenu(int Day) //add menu to days menu only if it is the next day, if its not then we've done somethign wrong
        {
            if (Day == this._menu.Count())
            {
                this._menu.Add(new Menu());
            }
        }

        public void addtoMenu(int meal, string foodName)
        {
            int Day = this._menu.Count() - 1;
            this._menu[Day].addFood(new FoodItem(foodName), meal);
        }

        public void setName(string Name) //shouldn't ever need to use this, should be setting name with the constructor
        {
            this._name = Name;
        }

        public string name()
        {
            return this._name;
        }

        public Menu getMenu(int Day)
        {
            return this._menu[Day];
        }

        public bool look(int day, string FoodName)
        {
            return this._menu[day].find(FoodName);
        }
    }

    class Menu
    {
        List<FoodItem> _breakfast;
        List<FoodItem> _lunch;
        List<FoodItem> _dinner;

        public Menu()
        {
            this._breakfast = new List<FoodItem>();
            this._lunch = new List<FoodItem>();
            this._dinner = new List<FoodItem>();
        }

        public List<FoodItem> loadMeal(int meal)
        {
            switch (meal)
            {
                case 0:
                    return this._breakfast;
                case 1:
                    return this._lunch;
                case 2:
                    return this._dinner;
                default:
                    return this._breakfast; //really should error out

            }
        }

        public void addFood(FoodItem OmNomNom, int Meal)
        {
            if (Meal == (int)Meals.BREAKFAST)
            {
                this._breakfast.Add(OmNomNom);
            }
            else if (Meal == (int)Meals.LUNCH)
            {
                this._lunch.Add(OmNomNom);
            }
            else if (Meal == (int)Meals.DINNER)
            {
                this._dinner.Add(OmNomNom);
            }
        }

        public bool find(string FoodName) //returns true if food name containing string is found
        {
            bool found = false;

            foreach (FoodItem food in this._breakfast)
            {
                if (food.name().Contains(FoodName))
                {
                    found = true;
                    return found;
                }

            }
            foreach (FoodItem food in this._lunch)
            {
                if (food.name().Contains(FoodName))
                {
                    found = true;
                    return found;
                }

            }
            foreach (FoodItem food in this._dinner)
            {
                if (food.name().Contains(FoodName))
                {
                    found = true;
                    return found;
                }

            }
            return found;
        }

    }

    class FoodItem
    {
        string _name;
        private bool _mHealthy;
        private bool _vegan;
        private bool _vegetarian;
        private bool _halal;
        private bool _glutenFree;
        private bool _spicy;

        public FoodItem(string Name)
        {
            this._name = Name;
            this._mHealthy = false;
            this._vegan = false;
            this._vegetarian = false;
            this._halal = false;
            this._glutenFree = false;
            this._spicy = false;
        }

        public string name()
        {
            return this._name;
        }

    }

    class SearchHit
    {
        private int _diningHall;
        private int _day;

        public string getDiningHall() {
            return BigData.hallNames[_diningHall];
        }
        public int getDay()
        {
            return _day;
        }

        public SearchHit(int Hall, int Day) //constructor for positive result
        {
            this._diningHall = Hall;
            this._day = Day;
        }
    }

}