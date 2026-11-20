INSERT INTO Users
(
    FirstName,
    LastName,
    Email,
    PasswordHash,
    Role
)
VALUES
(
    'Thabo',
    'Mokoena',
    'thabo.organiser@raceday.co.za',
    'PART1_SAMPLE_HASH_001',
    'Organiser'
),
(
    'Lerato',
    'Naidoo',
    'lerato.organiser@raceday.co.za',
    'PART1_SAMPLE_HASH_002',
    'Organiser'
),
(
    'Sipho',
    'Dlamini',
    'sipho.participant@raceday.co.za',
    'PART1_SAMPLE_HASH_003',
    'Participant'
),
(
    'Amahle',
    'Mthembu',
    'amahle.participant@raceday.co.za',
    'PART1_SAMPLE_HASH_004',
    'Participant'
);
GO

SELECT
    UserId,
    FirstName,
    LastName,
    Email,
    Role
FROM Users
ORDER BY UserId;

INSERT INTO Events
(
    OrganiserId,
    EventName,
    Description,
    EventDate,
    Venue,
    Province,
    Status
)
VALUES
(
    1,
    'Pretoria Spring Run',
    'Road running event for the Pretoria community.',
    '2027-09-12',
    'Loftus Versfeld Stadium',
    'Gauteng',
    'Upcoming'
),
(
    2,
    'Cape Town Community Cycle',
    'Community cycling event in Cape Town.',
    '2027-10-03',
    'Green Point',
    'Western Cape',
    'Upcoming'
),
(
    1,
    'Soweto Charity Walk',
    'Community charity walking event.',
    '2027-11-07',
    'Soweto Stadium',
    'Gauteng',
    'Upcoming'
);
GO

INSERT INTO Events
(
    OrganiserId,
    EventName,
    Description,
    EventDate,
    Venue,
    Province,
    Status
)
VALUES
(
    1,
    'Pretoria Spring Run',
    'Road running event for the Pretoria community.',
    '2027-09-12',
    'Loftus Versfeld Stadium',
    'Gauteng',
    'Upcoming'
),
(
    2,
    'Cape Town Community Cycle',
    'Community cycling event in Cape Town.',
    '2027-10-03',
    'Green Point',
    'Western Cape',
    'Upcoming'
),
(
    1,
    'Soweto Charity Walk',
    'Community charity walking event.',
    '2027-11-07',
    'Soweto Stadium',
    'Gauteng',
    'Upcoming'
);
GO

INSERT INTO Categories
(
    EventId,
    CategoryName,
    DistanceKm,
    MaxParticipants,
    EntryFee
)
VALUES
-- Pretoria Spring Run
(1, '5 km Fun Run', 5.00, 1000, 80.00),
(1, '10 km Road Race', 10.00, 1500, 150.00),
(1, '21.1 km Half Marathon', 21.10, 1000, 250.00),

-- Cape Town Community Cycle
(2, '20 km Community Ride', 20.00, 500, 100.00),
(2, '40 km Challenge Ride', 40.00, 750, 180.00),
(2, '80 km Endurance Ride', 80.00, 500, 300.00),

-- Soweto Charity Walk
(3, '3 km Family Walk', 3.00, 1000, 50.00),
(3, '5 km Community Walk', 5.00, 1500, 70.00),
(3, '10 km Charity Walk', 10.00, 1000, 100.00);
GO

INSERT INTO Routes
(
    EventId,
    RouteName,
    DistanceKm,
    StartLocation,
    FinishLocation,
    RouteDescription,
    MapUrl
)
VALUES
(
    1,
    'Pretoria City Route',
    10.00,
    'Loftus Versfeld Stadium',
    'Loftus Versfeld Stadium',
    'City road route through central Pretoria.',
    'https://example.com/pretoriaspringrun'
),
(
    2,
    'Cape Town Coastal Route',
    40.00,
    'Green Point',
    'Green Point',
    'Coastal cycling route around Cape Town.',
    'https://example.com/capetowncycle'
),
(
    3,
    'Soweto Community Route',
    5.00,
    'Soweto Stadium',
    'Soweto Stadium',
    'Community walking route through Soweto.',
    'https://example.com/sowetowalk'
);
GO

INSERT INTO Enrollments
(
    ParticipantId,
    CategoryId,
    Status,
    BibNumber
)
VALUES
(
    3,
    2,
    'Confirmed',
    101
),
(
    4,
    1,
    'Confirmed',
    102
),
(
    3,
    7,
    'Confirmed',
    201
),
(
    4,
    8,
    'Confirmed',
    202
);
GO

INSERT INTO Results
(
    EnrollmentId,
    FinishTime,
    Position,
    PaceMinutesPerKm
)
VALUES
(
    1,
    '00:52:30',
    15,
    5.25
),
(
    2,
    '00:27:45',
    8,
    5.55
),
(
    3,
    '00:35:20',
    12,
    7.07
),
(
    4,
    '00:36:10',
    18,
    7.23
);
GO

SELECT * FROM Results;

SELECT
    e.EventName,
    c.CategoryName,
    u.FirstName + ' ' + u.LastName AS Participant,
    en.BibNumber,
    en.Status
FROM Enrollments en
INNER JOIN Users u
    ON en.ParticipantId = u.UserId
INNER JOIN Categories c
    ON en.CategoryId = c.CategoryId
INNER JOIN Events e
    ON c.EventId = e.EventId
ORDER BY e.EventDate;