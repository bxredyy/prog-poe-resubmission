-- ============================================================
-- CyberSecurityBot - MySQL schema
-- Run against an empty database created via:
--     CREATE DATABASE cyberbot CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
--     CREATE USER 'cyberbot'@'localhost' IDENTIFIED BY 'cyberbot';
--     GRANT ALL PRIVILEGES ON cyberbot.* TO 'cyberbot'@'localhost';
--     FLUSH PRIVILEGES;
-- ============================================================

USE cyberbot;

CREATE TABLE IF NOT EXISTS tasks (
    id INT PRIMARY KEY AUTO_INCREMENT,
    title VARCHAR(200) NOT NULL,
    description TEXT,
    status VARCHAR(20) NOT NULL DEFAULT 'Pending',
    reminder_at DATETIME NULL,
    created_at DATETIME NOT NULL,
    completed_at DATETIME NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS activity_log (
    id INT PRIMARY KEY AUTO_INCREMENT,
    ts DATETIME NOT NULL,
    category VARCHAR(40) NOT NULL,
    description VARCHAR(500) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS quiz_attempts (
    id INT PRIMARY KEY AUTO_INCREMENT,
    score INT NOT NULL,
    total INT NOT NULL,
    completed_at DATETIME NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
