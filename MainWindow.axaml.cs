using Avalonia.Controls;
using Avalonia.Interactivity;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Media;

namespace SalaSportUI
{
    public partial class MainWindow : Window
    {
        private string connectionString = "server=localhost;user=root;password=Parola_Ta;database=sala_sport";

        private ObservableCollection<string> clientiItems = new();
        private ObservableCollection<string> rezervariItems = new();
        private ObservableCollection<string> programariItems = new();
        private ObservableCollection<KeyValuePair<int, string>> clientiComboItems = new();

        public MainWindow()
        {
            InitializeComponent();
            ClientiListBox.ItemsSource = null;

            ClientiListBox.ItemsSource = clientiItems;
            RezervariListBox.ItemsSource = rezervariItems;
            ProgramariListBox.ItemsSource = programariItems;
            RezervariClientComboBox.ItemsSource = clientiComboItems;

            /*
            LoadClienti();
            LoadRezervari();
            LoadProgramari();
            */
        }
        private bool ValidateTextBox(TextBox textBox)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Background = Brushes.LightPink;
                return false;
            }
            else
            {
                textBox.Background = Brushes.White;
                return true;
            }
        }


        private void LoadClienti()
        {
            clientiItems.Clear();
            clientiComboItems.Clear();

            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                string query = "SELECT id, nume, prenume, email, telefon FROM clienti";
                using var cmd = new MySqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string nume = reader.IsDBNull(1) ? "(fără nume)" : reader.GetString(1);
                    string prenume = reader.IsDBNull(2) ? "(fără prenume)" : reader.GetString(2);
                    string email = reader.IsDBNull(3) ? "(fără email)" : reader.GetString(3);
                    string telefon = reader.IsDBNull(4) ? "(fără telefon)" : reader.GetString(4);
                    string numeComplet = $"{nume} {prenume}";
                    string clientAfisare = $"{id}: {numeComplet} | Email: {email} | Telefon: {telefon}";
                    clientiItems.Add($"{clientAfisare}");                    
                    clientiComboItems.Add(new KeyValuePair<int, string>(id, numeComplet));
                }
            }
            catch (Exception ex)
            {
                clientiItems.Add("Eroare la încărcare clienți: " + ex.Message);
            }
        }

        private void OnAddClientClick(object? sender, RoutedEventArgs e)
        {
            string nume = ClientNumeTextBox.Text.Trim();
            string prenume = ClientPrenumeTextBox.Text.Trim();
            string email = ClientEmailTextBox.Text.Trim();
            string telefon = ClientTelefonTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(nume) || string.IsNullOrWhiteSpace(prenume))
            {
                ClientNumeTextBox.Background = string.IsNullOrWhiteSpace(nume) 
                    ? Brushes.LightPink 
                    : Brushes.White;

                ClientPrenumeTextBox.Background = string.IsNullOrWhiteSpace(prenume) 
                    ? Brushes.LightPink 
                    : Brushes.White;

                return;
            }
            else
            {
                ClientNumeTextBox.Background = Brushes.White;
                ClientPrenumeTextBox.Background = Brushes.White;
            }

            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                string query = "INSERT INTO clienti (nume, prenume, email, telefon) VALUES (@nume, @prenume, @email, @telefon)";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@nume", nume);
                cmd.Parameters.AddWithValue("@prenume", prenume);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@telefon", telefon);
                cmd.ExecuteNonQuery();

                ClientNumeTextBox.Text = "";
                ClientPrenumeTextBox.Text = "";
                ClientEmailTextBox.Text = "";
                ClientTelefonTextBox.Text = "";

                LoadClienti();
            }
            catch (Exception ex)
            {
                clientiItems.Add("Eroare la adăugare client: " + ex.Message);
            }
        }
        private void OnAfiseazaClientiClick(object? sender, RoutedEventArgs e)
        {
            LoadClienti();
        }
        private void OnModificaClientClick(object? sender, RoutedEventArgs e)
        {
            if (ClientiListBox.SelectedItem == null)
                return;

            string selectedItem = ClientiListBox.SelectedItem.ToString();
            int id = int.Parse(selectedItem.Split(':')[0]);

            string nume = ClientNumeTextBox.Text.Trim();
            string prenume = ClientPrenumeTextBox.Text.Trim();
            string email = ClientEmailTextBox.Text.Trim();
            string telefon = ClientTelefonTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(nume) || string.IsNullOrWhiteSpace(prenume))
                return;

            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                string query = @"UPDATE clienti 
                                SET nume = @nume, prenume = @prenume, email = @email, telefon = @telefon 
                                WHERE id = @id";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@nume", nume);
                cmd.Parameters.AddWithValue("@prenume", prenume);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@telefon", telefon);
                cmd.Parameters.AddWithValue("@id", id);

                cmd.ExecuteNonQuery();

                LoadClienti();
            }
            catch (Exception ex)
            {
                ClientiListBox.ItemsSource = new List<string> { "Eroare la modificare client: " + ex.Message };
            }
        }
        private void OnStergeClientClick(object? sender, RoutedEventArgs e)
        {
            if (ClientiListBox.SelectedItem == null)
                return;

            string selectedItem = ClientiListBox.SelectedItem.ToString();
            int id = int.Parse(selectedItem.Split(':')[0]);

            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                string query = "DELETE FROM clienti WHERE id = @id";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();

                LoadClienti();
            }
            catch (Exception ex)
            {
                ClientiListBox.ItemsSource = new List<string> { "Eroare la ștergere client: " + ex.Message };
            }
        }


        private void LoadRezervari()
        {
            rezervariItems.Clear();

            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                string query = @"
                    SELECT r.id, c.nume, c.prenume, r.data_rezervare, r.ora_inceput, r.ora_sfarsit, r.sala
                    FROM rezervari r
                    JOIN clienti c ON r.client_id = c.id";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string numeComplet = $"{reader.GetString(1)} {reader.GetString(2)}";
                    DateTime data = reader.GetDateTime(3);
                    TimeSpan oraStart = reader.GetTimeSpan(4);
                    TimeSpan oraSfarsit = reader.GetTimeSpan(5);
                    string sala = reader.GetString(6);

                    rezervariItems.Add($"{id}: {numeComplet} | {data:dd-MM-yyyy} {oraStart:hh\\:mm} - {oraSfarsit:hh\\:mm} | Sala: {sala}");
                }
            }
            catch (Exception ex)
            {
                rezervariItems.Add("Eroare la încărcare rezervări: " + ex.Message);
            }
        }

        private void OnAddRezervareClick(object? sender, RoutedEventArgs e)
        {
            if (RezervariClientComboBox.SelectedItem is not KeyValuePair<int, string> selectedClient)
                return;

            int clientId = selectedClient.Key;
            DateTime? dataRezervare = RezervariDataPicker.SelectedDate?.Date;
            string oraStartText = RezervariOraStartTextBox.Text.Trim();
            string oraSfarsitText = RezervariOraSfarsitTextBox.Text.Trim();
            string sala = RezervariSalaTextBox.Text.Trim();

            if (dataRezervare == null || string.IsNullOrWhiteSpace(oraStartText) || string.IsNullOrWhiteSpace(oraSfarsitText) || string.IsNullOrWhiteSpace(sala))
                return;

            if (!TimeSpan.TryParse(oraStartText, out TimeSpan oraStart) || !TimeSpan.TryParse(oraSfarsitText, out TimeSpan oraSfarsit))
                return;

            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                string query = @"INSERT INTO rezervari (client_id, data_rezervare, ora_inceput, ora_sfarsit, sala) 
                                VALUES (@client_id, @data_rezervare, @ora_inceput, @ora_sfarsit, @sala)";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@client_id", clientId);
                cmd.Parameters.AddWithValue("@data_rezervare", dataRezervare.Value);
                cmd.Parameters.AddWithValue("@ora_inceput", oraStart);
                cmd.Parameters.AddWithValue("@ora_sfarsit", oraSfarsit);
                cmd.Parameters.AddWithValue("@sala", sala);

                cmd.ExecuteNonQuery();

                RezervariDataPicker.SelectedDate = null;
                RezervariOraStartTextBox.Text = "";
                RezervariOraSfarsitTextBox.Text = "";
                RezervariSalaTextBox.Text = "";

                LoadRezervari();
            }
            catch (Exception ex)
            {
                rezervariItems.Add("Eroare la adăugare rezervare: " + ex.Message);
            }
        }
        private void OnAfisareRezervariClick(object? sender, RoutedEventArgs e)
        {
            LoadRezervari();
        }

        private void LoadProgramari()
        {
            programariItems.Clear();

            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                string query = @"
                    SELECT p.id, c.nume, c.prenume, p.data, p.ora, p.antrenor, p.activitate
                    FROM programari p
                    JOIN clienti c ON p.client_id = c.id";

                using var cmd = new MySqlCommand(query, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string numeComplet = $"{reader.GetString(1)} {reader.GetString(2)}";
                    DateTime data = reader.GetDateTime(3);
                    TimeSpan ora = reader.GetTimeSpan(4);
                    string antrenor = reader.GetString(5);
                    string activitate = reader.GetString(6);

                    programariItems.Add($"{id}: {numeComplet} | {data:dd-MM-yyyy} {ora:hh\\:mm} | Antrenor: {antrenor} | Activitate: {activitate}");
                }
            }
            catch (Exception ex)
            {
                programariItems.Add("Eroare la încărcare programări: " + ex.Message);
            }
        }

        private void OnAddProgramareClick(object? sender, RoutedEventArgs e)
        {
            if (!int.TryParse(ProgramareClientIdTextBox.Text.Trim(), out int clientId))
                return;

            if (!DateTime.TryParse(ProgramareDataTextBox.Text.Trim(), out DateTime data))
                return;

            if (!TimeSpan.TryParse(ProgramareOraTextBox.Text.Trim(), out TimeSpan ora))
                return;

            string antrenor = ProgramareAntrenorTextBox.Text.Trim();
            string activitate = ProgramareActivitateTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(antrenor) || string.IsNullOrWhiteSpace(activitate))
                return;

            try
            {
                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                string query = @"INSERT INTO programari (client_id, data, ora, antrenor, activitate)
                                VALUES (@client_id, @data, @ora, @antrenor, @activitate)";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@client_id", clientId);
                cmd.Parameters.AddWithValue("@data", data);
                cmd.Parameters.AddWithValue("@ora", ora);
                cmd.Parameters.AddWithValue("@antrenor", antrenor);
                cmd.Parameters.AddWithValue("@activitate", activitate);

                cmd.ExecuteNonQuery();

                ProgramareClientIdTextBox.Text = "";
                ProgramareDataTextBox.Text = "";
                ProgramareOraTextBox.Text = "";
                ProgramareAntrenorTextBox.Text = "";
                ProgramareActivitateTextBox.Text = "";

                LoadProgramari();
            }
            catch (Exception ex)
            {
                programariItems.Add("Eroare la adăugare programare: " + ex.Message);
            }
        }
        private void OnAfisareProgramariClick(object? sender, RoutedEventArgs e)
        {
            LoadProgramari();
        }
    }
}
