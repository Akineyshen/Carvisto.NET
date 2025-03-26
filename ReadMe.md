# 🚗 Platforma Wspólnych Podróży

&#x20; &#x20;

## 📌 Opis projektu

**Platforma Wspólnych Podróży** to innowacyjna strona internetowa, która umożliwia wyszukiwanie współpasażerów i organizowanie wspólnych przejazdów. Użytkownicy mogą znajdować kierowców lub pasażerów, uzgadniać szczegóły podróży oraz zawierać cyfrowe umowy, co zapewnia bezpieczeństwo i wygodę.

## 🚀 Funkcjonalność

### 🔹 1. Rejestracja i autoryzacja

- Rejestracja użytkowników
- Logowanie
- Przechowywanie danych użytkowników w bazie danych

### 🔹 2. Tworzenie podróży (kierowca)

- Wprowadzanie danych pojazdu (marka, model)
- Podawanie danych kontaktowych (imię, telefon)
- Określenie ceny za podróż
- Wprowadzenie trasy (skąd – dokąd) oraz daty wyjazdu
- Zapis informacji w bazie danych

### 🔹 3. Wyszukiwanie podróży (pasażer)

- Podanie informacji o pasażerze (imię, telefon)
- Filtrowanie dostępnych podróży według trasy, daty i ceny
- Wybór odpowiedniej opcji i wysyłanie zapytania do kierowcy

### 🔹 4. Umowa podróży

- Automatyczne generowanie umowy między kierowcą a pasażerem
- Tworzenie umowy w formacie PDF z danymi uczestników i szczegółami podróży
- Przechowywanie umowy w bazie danych oraz możliwość jej pobrania

### 🔹 5. Baza danych

- Przechowywanie informacji o użytkownikach, podróżach i umowach
- Powiązanie tabel w celu zapewnienia wygodnego wyszukiwania i zarządzania podróżami

### 🔹 6. Dodatkowe funkcje

- Oceny użytkowników
- Opinie i komentarze

## 🎯 Cel projektu

Naszym celem jest stworzenie bezpiecznej i wygodnej platformy do organizowania wspólnych podróży. Ułatwiamy wyszukiwanie współpasażerów oraz prawne uregulowanie przejazdów, zwiększając komfort i bezpieczeństwo podróżujących.

## 🛠 Technologie

- **Backend:** C# (.NET Core / .NET 6+)
- **Frontend:** JavaScript
- **Baza danych:** SQLite
- **Architektura:** MVC

## 📚 Wykorzystane biblioteki

- **ASP.NET Core** – framework do budowy aplikacji webowych
- **Entity Framework Core** – ORM do obsługi bazy danych
- **ClaimIdentity** – zarządzanie użytkownikami i autoryzacją

## 🛠 Instrukcja instalacji i konfiguracji

### 1️⃣ Klonowanie repozytorium

```sh
 git clone https://github.com/user/repo.git
 cd repo
```

### 2️⃣ Migracje bazy danych

```sh
 dotnet ef database update
```

### 3️⃣ Uruchomienie projektu

```sh
 dotnet run
```