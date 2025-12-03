BEGIN TRANSACTION;

IF NOT EXISTS (
    SELECT 1 FROM Items
    WHERE Name = 'Sword' AND Type = 'Weapon' AND Attack = 5
)
BEGIN
INSERT INTO Items (Name, Type, Attack, Defense, Weight, Value)
VALUES ('Sword', 'Weapon', 5, 0, 3.50, 100);

DECLARE @SwordId INT = SCOPE_IDENTITY();

INSERT INTO Equipments (WeaponId, ArmorId)
VALUES (@SwordId, NULL);

DECLARE @EquipmentId INT = SCOPE_IDENTITY();

    -- Assign equipment to player 1 only if player exists
    IF EXISTS (SELECT 1 FROM Players WHERE Id = 1)
BEGIN
UPDATE Players
SET EquipmentId = @EquipmentId
WHERE Id = 1;
END
END

COMMIT TRANSACTION;