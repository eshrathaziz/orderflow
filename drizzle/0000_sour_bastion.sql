CREATE TABLE `categories` (
	`id` int AUTO_INCREMENT NOT NULL,
	`ownerId` int NOT NULL,
	`name` varchar(100) NOT NULL,
	`description` text,
	`isActive` boolean NOT NULL DEFAULT true,
	`createdAt` timestamp NOT NULL DEFAULT (now()),
	CONSTRAINT `categories_id` PRIMARY KEY(`id`),
	CONSTRAINT `categories_owner_name_uq` UNIQUE(`ownerId`,`name`)
);
--> statement-breakpoint
CREATE TABLE `customerRequests` (
	`id` int AUTO_INCREMENT NOT NULL,
	`ownerId` int NOT NULL,
	`customerId` int NOT NULL,
	`orderId` int,
	`requestNumber` varchar(40) NOT NULL,
	`type` enum('change','cancellation','delivery','billing','other') NOT NULL DEFAULT 'other',
	`priority` enum('low','medium','high','urgent') NOT NULL DEFAULT 'medium',
	`status` enum('open','in_progress','resolved','closed') NOT NULL DEFAULT 'open',
	`assignedTo` varchar(120),
	`description` text NOT NULL,
	`resolution` text,
	`createdAt` timestamp NOT NULL DEFAULT (now()),
	`updatedAt` timestamp NOT NULL DEFAULT (now()) ON UPDATE CURRENT_TIMESTAMP,
	CONSTRAINT `customerRequests_id` PRIMARY KEY(`id`),
	CONSTRAINT `requests_owner_number_uq` UNIQUE(`ownerId`,`requestNumber`)
);
--> statement-breakpoint
CREATE TABLE `customers` (
	`id` int AUTO_INCREMENT NOT NULL,
	`ownerId` int NOT NULL,
	`companyName` varchar(160) NOT NULL,
	`contactName` varchar(120) NOT NULL,
	`email` varchar(320) NOT NULL,
	`phone` varchar(50),
	`status` enum('active','inactive','prospect') NOT NULL DEFAULT 'active',
	`createdAt` timestamp NOT NULL DEFAULT (now()),
	`updatedAt` timestamp NOT NULL DEFAULT (now()) ON UPDATE CURRENT_TIMESTAMP,
	CONSTRAINT `customers_id` PRIMARY KEY(`id`),
	CONSTRAINT `customers_owner_email_uq` UNIQUE(`ownerId`,`email`)
);
--> statement-breakpoint
CREATE TABLE `inventory` (
	`id` int AUTO_INCREMENT NOT NULL,
	`ownerId` int NOT NULL,
	`productId` int NOT NULL,
	`quantityOnHand` int NOT NULL DEFAULT 0,
	`reservedQuantity` int NOT NULL DEFAULT 0,
	`updatedAt` timestamp NOT NULL DEFAULT (now()) ON UPDATE CURRENT_TIMESTAMP,
	CONSTRAINT `inventory_id` PRIMARY KEY(`id`),
	CONSTRAINT `inventory_product_uq` UNIQUE(`productId`)
);
--> statement-breakpoint
CREATE TABLE `inventoryTransactions` (
	`id` int AUTO_INCREMENT NOT NULL,
	`ownerId` int NOT NULL,
	`productId` int NOT NULL,
	`orderId` int,
	`type` enum('stock_in','stock_out','adjustment','reservation','release') NOT NULL,
	`quantityDelta` int NOT NULL,
	`quantityBefore` int NOT NULL,
	`quantityAfter` int NOT NULL,
	`notes` text,
	`createdAt` timestamp NOT NULL DEFAULT (now()),
	CONSTRAINT `inventoryTransactions_id` PRIMARY KEY(`id`)
);
--> statement-breakpoint
CREATE TABLE `orderItems` (
	`id` int AUTO_INCREMENT NOT NULL,
	`ownerId` int NOT NULL,
	`orderId` int NOT NULL,
	`productId` int NOT NULL,
	`skuSnapshot` varchar(64) NOT NULL,
	`productNameSnapshot` varchar(180) NOT NULL,
	`quantity` int NOT NULL,
	`unitPrice` decimal(12,2) NOT NULL,
	`taxRate` decimal(6,4) NOT NULL,
	`lineTotal` decimal(12,2) NOT NULL,
	CONSTRAINT `orderItems_id` PRIMARY KEY(`id`)
);
--> statement-breakpoint
CREATE TABLE `orders` (
	`id` int AUTO_INCREMENT NOT NULL,
	`ownerId` int NOT NULL,
	`customerId` int NOT NULL,
	`orderNumber` varchar(40) NOT NULL,
	`status` enum('created','confirmed','processing','shipped','delivered','completed') NOT NULL DEFAULT 'created',
	`subtotal` decimal(12,2) NOT NULL,
	`taxRate` decimal(6,4) NOT NULL,
	`taxAmount` decimal(12,2) NOT NULL,
	`totalAmount` decimal(12,2) NOT NULL,
	`shippingAddress` text,
	`notes` text,
	`createdAt` timestamp NOT NULL DEFAULT (now()),
	`updatedAt` timestamp NOT NULL DEFAULT (now()) ON UPDATE CURRENT_TIMESTAMP,
	CONSTRAINT `orders_id` PRIMARY KEY(`id`),
	CONSTRAINT `orders_owner_number_uq` UNIQUE(`ownerId`,`orderNumber`)
);
--> statement-breakpoint
CREATE TABLE `products` (
	`id` int AUTO_INCREMENT NOT NULL,
	`ownerId` int NOT NULL,
	`categoryId` int NOT NULL,
	`sku` varchar(64) NOT NULL,
	`name` varchar(180) NOT NULL,
	`description` text,
	`unitPrice` decimal(12,2) NOT NULL,
	`reorderLevel` int NOT NULL DEFAULT 0,
	`status` enum('active','inactive') NOT NULL DEFAULT 'active',
	`createdAt` timestamp NOT NULL DEFAULT (now()),
	`updatedAt` timestamp NOT NULL DEFAULT (now()) ON UPDATE CURRENT_TIMESTAMP,
	CONSTRAINT `products_id` PRIMARY KEY(`id`),
	CONSTRAINT `products_owner_sku_uq` UNIQUE(`ownerId`,`sku`)
);
--> statement-breakpoint
CREATE TABLE `requestHistory` (
	`id` int AUTO_INCREMENT NOT NULL,
	`ownerId` int NOT NULL,
	`customerRequestId` int NOT NULL,
	`authorName` varchar(120) NOT NULL,
	`previousStatus` varchar(24),
	`nextStatus` varchar(24),
	`message` text NOT NULL,
	`isVisibleToCustomer` boolean NOT NULL DEFAULT false,
	`createdAt` timestamp NOT NULL DEFAULT (now()),
	CONSTRAINT `requestHistory_id` PRIMARY KEY(`id`)
);
--> statement-breakpoint
CREATE TABLE `users` (
	`id` int AUTO_INCREMENT NOT NULL,
	`openId` varchar(64) NOT NULL,
	`name` text,
	`email` varchar(320),
	`loginMethod` varchar(64),
	`role` enum('user','admin') NOT NULL DEFAULT 'user',
	`createdAt` timestamp NOT NULL DEFAULT (now()),
	`updatedAt` timestamp NOT NULL DEFAULT (now()) ON UPDATE CURRENT_TIMESTAMP,
	`lastSignedIn` timestamp NOT NULL DEFAULT (now()),
	CONSTRAINT `users_id` PRIMARY KEY(`id`),
	CONSTRAINT `users_openId_unique` UNIQUE(`openId`)
);
--> statement-breakpoint
ALTER TABLE `categories` ADD CONSTRAINT `categories_ownerId_users_id_fk` FOREIGN KEY (`ownerId`) REFERENCES `users`(`id`) ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
ALTER TABLE `customerRequests` ADD CONSTRAINT `customerRequests_ownerId_users_id_fk` FOREIGN KEY (`ownerId`) REFERENCES `users`(`id`) ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
ALTER TABLE `customerRequests` ADD CONSTRAINT `customerRequests_customerId_customers_id_fk` FOREIGN KEY (`customerId`) REFERENCES `customers`(`id`) ON DELETE restrict ON UPDATE no action;--> statement-breakpoint
ALTER TABLE `customerRequests` ADD CONSTRAINT `customerRequests_orderId_orders_id_fk` FOREIGN KEY (`orderId`) REFERENCES `orders`(`id`) ON DELETE set null ON UPDATE no action;--> statement-breakpoint
ALTER TABLE `customers` ADD CONSTRAINT `customers_ownerId_users_id_fk` FOREIGN KEY (`ownerId`) REFERENCES `users`(`id`) ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
ALTER TABLE `inventory` ADD CONSTRAINT `inventory_ownerId_users_id_fk` FOREIGN KEY (`ownerId`) REFERENCES `users`(`id`) ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
ALTER TABLE `inventory` ADD CONSTRAINT `inventory_productId_products_id_fk` FOREIGN KEY (`productId`) REFERENCES `products`(`id`) ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
ALTER TABLE `inventoryTransactions` ADD CONSTRAINT `inventoryTransactions_ownerId_users_id_fk` FOREIGN KEY (`ownerId`) REFERENCES `users`(`id`) ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
ALTER TABLE `inventoryTransactions` ADD CONSTRAINT `inventoryTransactions_productId_products_id_fk` FOREIGN KEY (`productId`) REFERENCES `products`(`id`) ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
ALTER TABLE `inventoryTransactions` ADD CONSTRAINT `inventoryTransactions_orderId_orders_id_fk` FOREIGN KEY (`orderId`) REFERENCES `orders`(`id`) ON DELETE set null ON UPDATE no action;--> statement-breakpoint
ALTER TABLE `orderItems` ADD CONSTRAINT `orderItems_ownerId_users_id_fk` FOREIGN KEY (`ownerId`) REFERENCES `users`(`id`) ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
ALTER TABLE `orderItems` ADD CONSTRAINT `orderItems_orderId_orders_id_fk` FOREIGN KEY (`orderId`) REFERENCES `orders`(`id`) ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
ALTER TABLE `orderItems` ADD CONSTRAINT `orderItems_productId_products_id_fk` FOREIGN KEY (`productId`) REFERENCES `products`(`id`) ON DELETE restrict ON UPDATE no action;--> statement-breakpoint
ALTER TABLE `orders` ADD CONSTRAINT `orders_ownerId_users_id_fk` FOREIGN KEY (`ownerId`) REFERENCES `users`(`id`) ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
ALTER TABLE `orders` ADD CONSTRAINT `orders_customerId_customers_id_fk` FOREIGN KEY (`customerId`) REFERENCES `customers`(`id`) ON DELETE restrict ON UPDATE no action;--> statement-breakpoint
ALTER TABLE `products` ADD CONSTRAINT `products_ownerId_users_id_fk` FOREIGN KEY (`ownerId`) REFERENCES `users`(`id`) ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
ALTER TABLE `products` ADD CONSTRAINT `products_categoryId_categories_id_fk` FOREIGN KEY (`categoryId`) REFERENCES `categories`(`id`) ON DELETE restrict ON UPDATE no action;--> statement-breakpoint
ALTER TABLE `requestHistory` ADD CONSTRAINT `requestHistory_ownerId_users_id_fk` FOREIGN KEY (`ownerId`) REFERENCES `users`(`id`) ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
ALTER TABLE `requestHistory` ADD CONSTRAINT `requestHistory_customerRequestId_customerRequests_id_fk` FOREIGN KEY (`customerRequestId`) REFERENCES `customerRequests`(`id`) ON DELETE cascade ON UPDATE no action;--> statement-breakpoint
CREATE INDEX `requests_owner_status_priority_idx` ON `customerRequests` (`ownerId`,`status`,`priority`);--> statement-breakpoint
CREATE INDEX `requests_customer_idx` ON `customerRequests` (`customerId`);--> statement-breakpoint
CREATE INDEX `customers_owner_company_idx` ON `customers` (`ownerId`,`companyName`);--> statement-breakpoint
CREATE INDEX `inventory_owner_product_idx` ON `inventory` (`ownerId`,`productId`);--> statement-breakpoint
CREATE INDEX `inventory_tx_product_created_idx` ON `inventoryTransactions` (`productId`,`createdAt`);--> statement-breakpoint
CREATE INDEX `inventory_tx_order_idx` ON `inventoryTransactions` (`orderId`);--> statement-breakpoint
CREATE INDEX `order_items_order_idx` ON `orderItems` (`orderId`);--> statement-breakpoint
CREATE INDEX `order_items_product_idx` ON `orderItems` (`productId`);--> statement-breakpoint
CREATE INDEX `orders_owner_status_created_idx` ON `orders` (`ownerId`,`status`,`createdAt`);--> statement-breakpoint
CREATE INDEX `orders_customer_created_idx` ON `orders` (`customerId`,`createdAt`);--> statement-breakpoint
CREATE INDEX `products_owner_category_idx` ON `products` (`ownerId`,`categoryId`);--> statement-breakpoint
CREATE INDEX `request_history_request_created_idx` ON `requestHistory` (`customerRequestId`,`createdAt`);