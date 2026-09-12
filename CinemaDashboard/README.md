# CinemaDashboard — ASP.NET Core MVC + EF Core (Admin Area)

Matches the task brief from the video:
1. **Category** — Id, Name
2. **Cinema** — Id, Name, Address, Img
3. **Movie** — Id, Name, Des, Price, Status (enum), DateTime, MainImg, SubImages (gallery), List\<Actor\> (cast), CategoryId, CinemaId
4. **Actor** — Id, Name, Img
5. **Dashboard home page** — styled like the SB Admin Bootstrap template shown in the video (dark sidebar, stat cards, latest-movies table)

Everything lives under an **Admin area** (`/Admin/...`), matching the
`localhost:7134/Admin/Home/Index` URL shown in the recording. The root URL
`/` redirects straight to `/Admin/Home/Index`.

## Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB/Express/full)
- `dotnet tool install --global dotnet-ef` (if not already installed)

## Setup
```bash
cd CinemaDashboard
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```
Open the URL shown in the console, e.g. `https://localhost:xxxx/Admin/Home/Index`.

## Project layout
```
Models/            Category, Cinema, Actor, Movie, MovieImage, MovieActor, Enums/MovieStatus
Data/               ApplicationDbContext
Helpers/            FileUploadHelper (shared image save/delete logic)
Areas/Admin/
  Controllers/      Home, Category, Cinema, Actor, Movie (full CRUD)
  Views/            SB-Admin-style layout + Index/Create/Edit for each entity
wwwroot/uploads/    movies/, cinemas/, actors/ - uploaded images land here
```

## Notes on design decisions
- **Movie.Des** is mapped from the C# property `Description` via `[Column("Des")]`,
  so the database column is literally `Des` as specified, while the code stays readable.
- **Movie.Status** is an enum: `ComingSoon`, `NowShowing`, `Ended`. Adjust the values
  in `Models/Enums/MovieStatus.cs` if your brief specifies different ones.
- **SubImages** is modeled as a separate `MovieImage` table (one Movie → many images),
  since "SubImages" implies a gallery rather than a single field.
- **List\<Actor\>** is implemented as a classic many-to-many via the `MovieActor`
  join table (composite key `MovieId + ActorId`). The Movie form uses a multi-select
  list; hold Ctrl/Cmd to pick several actors.
- **Cinema/Actor "Img"** and **Movie "MainImg"/"SubImages"** are all stored as files
  under `wwwroot/uploads/{cinemas|actors|movies}` with a generated GUID filename;
  only the filename is stored in the DB.
- The dashboard layout uses Bootstrap 5 + Font Awesome from CDN to approximate the
  SB Admin look from the video (dark sidebar, colored stat cards). If your assignment
  requires the exact official "Start Bootstrap SB Admin" template files, let me know
  and I can wire in the real template instead of this Bootstrap-based approximation.

## Things worth double-checking against your exact assignment brief
- Field lengths/types for `Category.Name`, `Cinema.Name/Address`, `Actor.Name` are
  reasonable defaults — tell me if the brief specifies exact limits.

## Authentication (ASP.NET Core Identity)
The whole `/Admin` area is now locked behind a login, using ASP.NET Core Identity
with a custom (hand-written, not scaffolded) login page.

- `Models/ApplicationUser.cs` — extends `IdentityUser` (has a spare `FullName` field
  for later use).
- `Data/ApplicationDbContext.cs` — now inherits `IdentityDbContext<ApplicationUser>`,
  so `AspNetUsers`, `AspNetRoles`, etc. are created alongside your existing tables.
- `Data/IdentitySeeder.cs` — runs once on startup, creates an **Admin** role and a
  default admin account if none exists:
  - **Email:** `admin@cinema.local`
  - **Password:** `Admin@123`
  - **Change this password after your first login** (or edit the seeder before
    your first `dotnet run`).
- `Controllers/AccountController.cs` + `Views/Account/Login.cshtml` — plain login
  form (email + password + remember me) and a `Logout` POST action.
- Every controller in `Areas/Admin/Controllers` now has
  `[Authorize(Roles = "Admin")]` — visiting any admin page while logged out (or
  logged in as a non-admin) redirects to `/Account/Login`.
- The admin navbar (`Areas/Admin/Views/Shared/_Layout.cshtml`) shows the signed-in
  user's name and a Logout button.

### Re-run migrations after this change
Since the DbContext changed (new Identity tables), add a new migration:
```bash
dotnet ef migrations add AddIdentity
dotnet ef database update
dotnet run
```
Then open the app, get redirected to `/Account/Login`, and sign in with the seeded
admin credentials above.

### Notes / things to adjust for a real deployment
- The password rules in `Program.cs` (`AddIdentity` options) are relaxed for local
  testing. Tighten `RequireNonAlphanumeric`, `RequireUppercase`, `RequiredLength`,
  etc. before shipping anything real.
- There's no public "Register" page on purpose — this is an admin-only dashboard.
  If you need to create more admin accounts, either extend `IdentitySeeder` or add
  a simple "Create User" screen inside the Admin area (I can build that next if
  you want it).
- The seeded password is in plain text in `IdentitySeeder.cs` for convenience in
  this learning/exercise context — don't commit real credentials like this in a
  production app.

## Customer storefront (browse, cart, checkout)
The public site (root `/`) is now a real storefront, separate from the `/Admin`
dashboard — no login needed to browse or buy.

- **`/` or `/Home/Index`** — movie catalog (grid of cards) with "Details" and
  "Add to Cart" buttons on every movie.
- **`/Home/Details/{id}`** — full movie page: description, cast, gallery images,
  a quantity field, and "Add to Cart".
- **`/Cart`** — cart page: update quantity, remove a line, clear the cart, see
  the running total, "Proceed to Checkout".
- **`/Checkout`** — customer details form (name, email, phone) + order summary.
  "Place Order" saves an `Order` + `OrderItem` rows to the database and empties
  the cart.
- **`/Checkout/Confirmation/{orderId}`** — thank-you page with the order recap.

### How the cart works
- `Models/Cart/CartItem.cs` is a plain class (not a DB entity) held in the
  visitor's **Session** (`Services/CartService.cs` + `Helpers/SessionExtensions.cs`
  serialize it to JSON under the hood). Nothing is written to the database until
  checkout — so an abandoned cart never creates DB rows.
- `Order` / `OrderItem` (in `Models/`) are real EF Core entities that only get
  created when the customer places an order. `OrderItem` snapshots the movie's
  name/price at purchase time so historical orders stay accurate even if a movie
  is later edited or deleted from the admin dashboard.
- The navbar cart badge (`Views/Shared/_Layout.cshtml`) reads the live count via
  `@inject CartService` — no page refresh gymnastics needed, it's just server-rendered
  on every request.
- A "Staff Login" link in the storefront navbar goes to `/Account/Login` for admin
  access; customers never see or need it.

### One more migration needed
Since `Order`/`OrderItem` are new entities:
```bash
dotnet ef migrations add AddOrders
dotnet ef database update
dotnet run
```
(Run this after the `AddIdentity` migration from the previous step.)

### Things you may want to adjust
- There's no real payment integration — "Place Order" just records the order.
  If you need actual payment processing, that's a separate integration (e.g.
  Stripe) I can help wire in.
- Cart is session-based (per browser session), not tied to a customer account,
  since there's no customer login — only staff/admin login exists.
- Sub-images shown in the movie details gallery aren't a full lightbox — bumping
  that or adding an "Order History" screen for admins are natural next steps
  if you want them.
