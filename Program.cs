using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;


namespace Piramida
{
    public class Uczestnik
    {
        public int id;
        public List<int> przelozeni = new List<int>();
        public bool czySzefuje;
        public int iluSzefuje;
        int prowizja;

        public string Info()
        {
            string inf = this.id.ToString() + " " + this.Poziom().ToString() + " " + this.iluSzefuje.ToString() + " " + this.prowizja.ToString();
            return inf;
        }

        public void DodProwizje(int p)
        {
            this.prowizja += p;
        }

        public int BezposPrzelo()
        {
            int p;
            if (this.przelozeni.Count != 0) p = this.przelozeni[0];
            else p = 0;

            return p;
        }

        public int Poziom()
        {
            return this.przelozeni.Count;
        }

        public Uczestnik(int i, List<int> Przel)
        {
            id = i;
            przelozeni = Przel.ToList();
            przelozeni.Reverse();
            prowizja = 0;
            czySzefuje = false;
            iluSzefuje = 0;

        }

    }

    public class Wplata
    {
       
        public int id;
        public int kwota;
        public Wplata(int id, int kwota)
        {
            this.id = id;
            this.kwota = kwota;
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            String Piramida = "piramida.xml";
            String Przelewy = "przelewy.xml";
            
            if (SprPliki(Piramida, Przelewy))
            {
                List<Uczestnik> ListaU = CzytajUczest(Piramida);
                List<Wplata> ListaW = CzytajWplaty(Przelewy);

                if (SprZgodnosc(ListaU, ListaW))
                {
                    Wplac(ListaU, Oblicz(ListaU, ListaW));
                    SprPrzelozonych(ListaU);
                    Wyswietl(ListaU);
                }
            }
        }
        
        static bool SprPliki(string p1, string p2)
        {
            if (File.Exists(p1) && File.Exists(p2))
            {
                return true;
            }
            else
            {
                Console.WriteLine("Błąd: Brak pliku/ów wejściowych " + p1 + " lub/i " + p2 +" .");
                Console.WriteLine("Naciśnij dowolny klawisz aby zamknąć...");
                Console.ReadKey();
                return false;
            }
            
        }

        static List<Wplata> CzytajWplaty(string Plik)
        {
            int id, kwota;
            List <Wplata> Kwoty = new List<Wplata>();
            XmlTextReader Czytaj = new XmlTextReader(Plik);
            Czytaj.WhitespaceHandling = WhitespaceHandling.None;

            while (Czytaj.Read())
            {
                if (2 == Czytaj.AttributeCount)
                {


                    id = Int32.Parse(Czytaj.GetAttribute("od"));
                    kwota = Int32.Parse(Czytaj.GetAttribute("kwota"));
                    Kwoty.Add(new Wplata(id, kwota));                    

                }
            }
            Czytaj.Close();
            return Kwoty;
        }

        static List<Uczestnik> CzytajUczest(string Plik)
        {
            int id;
            var ListaU = new List<Uczestnik>();
            var ListaId = new List<int>();
            
            XmlTextReader Czytaj = new XmlTextReader(Plik);
            Czytaj.WhitespaceHandling = WhitespaceHandling.None;
            
            while (Czytaj.Read())
            {
                    switch (Czytaj.NodeType)
                    {
                        case XmlNodeType.Element:
                        if (1 == Czytaj.AttributeCount)
                        {
                            id = Int32.Parse(Czytaj.GetAttribute("id"));
                            ListaU.Add(new Uczestnik(id, ListaId));
                            ListaId.Add(id);
                        }
                        if (Czytaj.IsEmptyElement)
                        {
                            ListaId.RemoveAt(ListaId.Count-1);
                        }
                            break;

                        case XmlNodeType.EndElement:
                        if (ListaId.Count > 0)
                        {
                            ListaId.RemoveAt(ListaId.Count - 1);
                        }
                        break;
                    }
                
            }
            Czytaj.Close();
            return ListaU;
        }

        static void Wyswietl(List<Uczestnik> Lista)
        {
            IEnumerable<Uczestnik> kolejnosc = Lista.OrderBy(x => x.id);
            foreach (Uczestnik x in kolejnosc)
            {
                Console.WriteLine(x.Info());
            }           
            Console.ReadKey();
        }

        static List<Wplata> Oblicz(List<Uczestnik> ListaUcz,List<Wplata> ListaWplat)
        {
            int id, id2, hajs1, hajs2;
            List<Wplata> ListaNaleznosci = new List<Wplata>();
            for(int i = 0; i < ListaWplat.Count(); i++)
            {
                id = ListaWplat[i].id;
                hajs1 = ListaWplat[i].kwota;
                Uczestnik Ucz = ListaUcz.Find(x => x.id == id);
               //Czy wpłata jest od szefa wszystkich szefów
                if(Ucz.przelozeni.Count() == 0)
                {
                    //sprawdź czy jest już na liście
                    id2 = ListaNaleznosci.FindIndex(x => x.id == Ucz.id);
                    if(id2 == (-1))  //Nie ma go na liście
                    {
                        Wplata Wpl = new Wplata(id2, hajs1);
                        ListaNaleznosci.Add(Wpl);
                    }
                    else  //jest na liście
                    {
                        ListaNaleznosci[id2].kwota += hajs1;
                    }
                }
                else //Reszta wpłat
                {
                    for (int j = Ucz.przelozeni.Count(); j > 0 ; j--)
                    {
                      
                        if(j == 1)
                        {
                            hajs2 = hajs1;
                        }
                        else
                        {
                            hajs2 = hajs1 / 2;
                            hajs1 = hajs1 - hajs2;
                        }

                       id = Ucz.przelozeni[j-1];
                       id2 = ListaNaleznosci.FindIndex(x => x.id == id);
                        
                        if (id2 == (-1))  //Nie ma go na liście
                        {
                            Wplata Wpl = new Wplata(id, hajs2);
                            ListaNaleznosci.Add(Wpl);
                        }
                        else  //jest na liście
                        {
                            ListaNaleznosci[id2].kwota += hajs2;
                        }
                    }
                }
    }

            return ListaNaleznosci;
        }

        static void Wplac(List<Uczestnik> ListaUcz, List<Wplata> ListaNal)
        {
            int id;
            for (int i = 0; i < ListaNal.Count(); i++)
            {
                id = ListaNal[i].id;
                id = ListaUcz.FindIndex(x => x.id == id);
                ListaUcz[id].DodProwizje(ListaNal[i].kwota);
            }
        }

        static void SprPrzelozonych(List<Uczestnik> ListaUcz)
        { 
            int id;
            for (int i = 0; i < ListaUcz.Count(); i++)
            {
                if (ListaUcz[i].przelozeni.Any())
                {
                    id = ListaUcz[i].przelozeni[0];
                    ListaUcz[ListaUcz.FindIndex(x => x.id == id)].czySzefuje = true;
                }
            }

            for (int i = 0; i < ListaUcz.Count(); i++)
            {
                if (!ListaUcz[i].czySzefuje)
                {
                   for(int j = 0; j < ListaUcz[i].przelozeni.Count(); j++)
                    {
                        id = ListaUcz[i].przelozeni[j];
                        ListaUcz[ListaUcz.FindIndex(x => x.id == id)].iluSzefuje++;
                    }                   
                }
            }
         } 

        static bool SprZgodnosc(List<Uczestnik> ListaU, List<Wplata> ListaW)
        {
            int id;
            for (int i = ListaW.Count()-1; i > 0; --i)
            {
                id = ListaW[i].id;
                if((-1) == ListaU.FindIndex(x => x.id == id))
                {
                    Console.WriteLine("Błąd: Istnieją wpłaty od niezidentyfikowanych uczestników.");
                    Console.WriteLine("Naciśnij dowolny klawisz aby zamknąć...");
                    Console.ReadKey();
                    return false;
                }
            }
            return true;
        }
    }
}
