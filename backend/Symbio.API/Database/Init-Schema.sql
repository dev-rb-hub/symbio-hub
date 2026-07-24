CREATE TABLE IF NOT EXISTS Jobs (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Description TEXT NOT NULL,
    ClientName TEXT NOT NULL,
    ClientSurname TEXT NOT NULL,
    Budget REAL NOT NULL,
    ContactEmail TEXT NOT NULL,
    IsPublished INTEGER NOT NULL DEFAULT 1,
    PostedAt TEXT NOT NULL
);

INSERT INTO Jobs (Title, Description, ClientName, ClientSurname, Budget, ContactEmail, IsPublished, PostedAt)
VALUES
('Regional Retail Website Refresh', 'Build a mobile-first homepage and checkout experience for a small NSW retail brand.', 'Harper', 'Bright', 9500.00, 'contact@harperbright.com', 1, '2026-07-19T00:00:00Z'),
('Local Healthcare Data Dashboard', 'Create a lightweight reporting dashboard for a regional practice using anonymised patient metrics.', 'Jade', 'Taylor', 14500.00, 'jade.taylor@coastalhealth.au', 1, '2026-07-12T00:00:00Z'),
('Food Delivery Loyalty Campaign', 'Design and build a customer loyalty landing page with signup flow and campaign analytics.', 'Miles', 'Kerr', 7200.00, 'miles@harvestdeli.au', 1, '2026-07-22T00:00:00Z');
