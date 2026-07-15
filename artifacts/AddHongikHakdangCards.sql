START TRANSACTION;
CREATE TABLE `hongik_hakdang_card_collections` (
    `Id` bigint NOT NULL AUTO_INCREMENT,
    `source_key` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `name` varchar(300) CHARACTER SET utf8mb4 NOT NULL,
    `sort_order` int NOT NULL,
    `is_active` tinyint(1) NOT NULL,
    `last_seen_at_utc` datetime(6) NOT NULL,
    `created_at_utc` datetime(6) NOT NULL,
    `updated_at_utc` datetime(6) NOT NULL,
    CONSTRAINT `PK_hongik_hakdang_card_collections` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `hongik_hakdang_cards` (
    `Id` bigint NOT NULL AUTO_INCREMENT,
    `source_key` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
    `title` varchar(500) CHARACTER SET utf8mb4 NULL,
    `description` text CHARACTER SET utf8mb4 NULL,
    `original_image_url` varchar(1500) CHARACTER SET utf8mb4 NOT NULL,
    `thumbnail_image_url` varchar(1500) CHARACTER SET utf8mb4 NULL,
    `related_url` varchar(1500) CHARACTER SET utf8mb4 NULL,
    `local_image_path` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `image_content_type` varchar(100) CHARACTER SET utf8mb4 NULL,
    `image_size_bytes` bigint NULL,
    `image_sha256` varchar(64) CHARACTER SET utf8mb4 NULL,
    `image_download_status` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
    `image_download_error` varchar(1000) CHARACTER SET utf8mb4 NULL,
    `image_downloaded_at_utc` datetime(6) NULL,
    `is_active` tinyint(1) NOT NULL,
    `last_seen_at_utc` datetime(6) NOT NULL,
    `created_at_utc` datetime(6) NOT NULL,
    `updated_at_utc` datetime(6) NOT NULL,
    CONSTRAINT `PK_hongik_hakdang_cards` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `hongik_hakdang_card_collection_items` (
    `collection_id` bigint NOT NULL,
    `card_id` bigint NOT NULL,
    `sort_order` int NOT NULL,
    `is_active` tinyint(1) NOT NULL,
    `last_seen_at_utc` datetime(6) NOT NULL,
    CONSTRAINT `PK_hongik_hakdang_card_collection_items` PRIMARY KEY (`collection_id`, `card_id`),
    CONSTRAINT `FK_hh_card_items_collections` FOREIGN KEY (`collection_id`) REFERENCES `hongik_hakdang_card_collections` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_hh_card_items_cards` FOREIGN KEY (`card_id`) REFERENCES `hongik_hakdang_cards` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_hh_card_items_card_active` ON `hongik_hakdang_card_collection_items` (`card_id`, `is_active`);

CREATE INDEX `IX_hh_card_items_collection_active_order` ON `hongik_hakdang_card_collection_items` (`collection_id`, `is_active`, `sort_order`);

CREATE INDEX `IX_hh_card_collections_active_order` ON `hongik_hakdang_card_collections` (`is_active`, `sort_order`);

CREATE UNIQUE INDEX `IX_hh_card_collections_source_key` ON `hongik_hakdang_card_collections` (`source_key`);

CREATE INDEX `IX_hh_cards_download_status` ON `hongik_hakdang_cards` (`image_download_status`);

CREATE INDEX `IX_hh_cards_active_last_seen` ON `hongik_hakdang_cards` (`is_active`, `last_seen_at_utc`);

CREATE UNIQUE INDEX `IX_hh_cards_source_key` ON `hongik_hakdang_cards` (`source_key`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260714143000_AddHongikHakdangCards', '9.0.0');

COMMIT;
