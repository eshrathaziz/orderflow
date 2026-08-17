import {
  boolean,
  decimal,
  index,
  int,
  mysqlEnum,
  mysqlTable,
  text,
  timestamp,
  uniqueIndex,
  varchar,
} from "drizzle-orm/mysql-core";

/** Built-in account table populated through the managed OAuth flow. */
export const users = mysqlTable("users", {
  id: int("id").autoincrement().primaryKey(),
  openId: varchar("openId", { length: 64 }).notNull().unique(),
  name: text("name"),
  email: varchar("email", { length: 320 }),
  loginMethod: varchar("loginMethod", { length: 64 }),
  role: mysqlEnum("role", ["user", "admin"]).default("user").notNull(),
  createdAt: timestamp("createdAt").defaultNow().notNull(),
  updatedAt: timestamp("updatedAt").defaultNow().onUpdateNow().notNull(),
  lastSignedIn: timestamp("lastSignedIn").defaultNow().notNull(),
});

/** Customers are tenant-scoped to the current managed account. */
export const customers = mysqlTable(
  "customers",
  {
    id: int("id").autoincrement().primaryKey(),
    ownerId: int("ownerId").notNull().references(() => users.id, { onDelete: "cascade" }),
    companyName: varchar("companyName", { length: 160 }).notNull(),
    contactName: varchar("contactName", { length: 120 }).notNull(),
    email: varchar("email", { length: 320 }).notNull(),
    phone: varchar("phone", { length: 50 }),
    status: mysqlEnum("status", ["active", "inactive", "prospect"]).default("active").notNull(),
    createdAt: timestamp("createdAt").defaultNow().notNull(),
    updatedAt: timestamp("updatedAt").defaultNow().onUpdateNow().notNull(),
  },
  table => [index("customers_owner_company_idx").on(table.ownerId, table.companyName), uniqueIndex("customers_owner_email_uq").on(table.ownerId, table.email)],
);

export const categories = mysqlTable(
  "categories",
  {
    id: int("id").autoincrement().primaryKey(),
    ownerId: int("ownerId").notNull().references(() => users.id, { onDelete: "cascade" }),
    name: varchar("name", { length: 100 }).notNull(),
    description: text("description"),
    isActive: boolean("isActive").default(true).notNull(),
    createdAt: timestamp("createdAt").defaultNow().notNull(),
  },
  table => [uniqueIndex("categories_owner_name_uq").on(table.ownerId, table.name)],
);

export const products = mysqlTable(
  "products",
  {
    id: int("id").autoincrement().primaryKey(),
    ownerId: int("ownerId").notNull().references(() => users.id, { onDelete: "cascade" }),
    categoryId: int("categoryId").notNull().references(() => categories.id, { onDelete: "restrict" }),
    sku: varchar("sku", { length: 64 }).notNull(),
    name: varchar("name", { length: 180 }).notNull(),
    description: text("description"),
    unitPrice: decimal("unitPrice", { precision: 12, scale: 2 }).notNull(),
    reorderLevel: int("reorderLevel").default(0).notNull(),
    status: mysqlEnum("status", ["active", "inactive"]).default("active").notNull(),
    createdAt: timestamp("createdAt").defaultNow().notNull(),
    updatedAt: timestamp("updatedAt").defaultNow().onUpdateNow().notNull(),
  },
  table => [uniqueIndex("products_owner_sku_uq").on(table.ownerId, table.sku), index("products_owner_category_idx").on(table.ownerId, table.categoryId)],
);

/** One current inventory balance per product; movement history is kept separately. */
export const inventory = mysqlTable(
  "inventory",
  {
    id: int("id").autoincrement().primaryKey(),
    ownerId: int("ownerId").notNull().references(() => users.id, { onDelete: "cascade" }),
    productId: int("productId").notNull().references(() => products.id, { onDelete: "cascade" }),
    quantityOnHand: int("quantityOnHand").default(0).notNull(),
    reservedQuantity: int("reservedQuantity").default(0).notNull(),
    updatedAt: timestamp("updatedAt").defaultNow().onUpdateNow().notNull(),
  },
  table => [uniqueIndex("inventory_product_uq").on(table.productId), index("inventory_owner_product_idx").on(table.ownerId, table.productId)],
);

export const orders = mysqlTable(
  "orders",
  {
    id: int("id").autoincrement().primaryKey(),
    ownerId: int("ownerId").notNull().references(() => users.id, { onDelete: "cascade" }),
    customerId: int("customerId").notNull().references(() => customers.id, { onDelete: "restrict" }),
    orderNumber: varchar("orderNumber", { length: 40 }).notNull(),
    status: mysqlEnum("status", ["created", "confirmed", "processing", "shipped", "delivered", "completed"]).default("created").notNull(),
    subtotal: decimal("subtotal", { precision: 12, scale: 2 }).notNull(),
    taxRate: decimal("taxRate", { precision: 6, scale: 4 }).notNull(),
    taxAmount: decimal("taxAmount", { precision: 12, scale: 2 }).notNull(),
    totalAmount: decimal("totalAmount", { precision: 12, scale: 2 }).notNull(),
    shippingAddress: text("shippingAddress"),
    notes: text("notes"),
    createdAt: timestamp("createdAt").defaultNow().notNull(),
    updatedAt: timestamp("updatedAt").defaultNow().onUpdateNow().notNull(),
  },
  table => [uniqueIndex("orders_owner_number_uq").on(table.ownerId, table.orderNumber), index("orders_owner_status_created_idx").on(table.ownerId, table.status, table.createdAt), index("orders_customer_created_idx").on(table.customerId, table.createdAt)],
);

/** Price, SKU and name snapshots retain the historical commercial record. */
export const orderItems = mysqlTable(
  "orderItems",
  {
    id: int("id").autoincrement().primaryKey(),
    ownerId: int("ownerId").notNull().references(() => users.id, { onDelete: "cascade" }),
    orderId: int("orderId").notNull().references(() => orders.id, { onDelete: "cascade" }),
    productId: int("productId").notNull().references(() => products.id, { onDelete: "restrict" }),
    skuSnapshot: varchar("skuSnapshot", { length: 64 }).notNull(),
    productNameSnapshot: varchar("productNameSnapshot", { length: 180 }).notNull(),
    quantity: int("quantity").notNull(),
    unitPrice: decimal("unitPrice", { precision: 12, scale: 2 }).notNull(),
    taxRate: decimal("taxRate", { precision: 6, scale: 4 }).notNull(),
    lineTotal: decimal("lineTotal", { precision: 12, scale: 2 }).notNull(),
  },
  table => [index("order_items_order_idx").on(table.orderId), index("order_items_product_idx").on(table.productId)],
);

export const inventoryTransactions = mysqlTable(
  "inventoryTransactions",
  {
    id: int("id").autoincrement().primaryKey(),
    ownerId: int("ownerId").notNull().references(() => users.id, { onDelete: "cascade" }),
    productId: int("productId").notNull().references(() => products.id, { onDelete: "cascade" }),
    orderId: int("orderId").references(() => orders.id, { onDelete: "set null" }),
    type: mysqlEnum("type", ["stock_in", "stock_out", "adjustment", "reservation", "release"]).notNull(),
    quantityDelta: int("quantityDelta").notNull(),
    quantityBefore: int("quantityBefore").notNull(),
    quantityAfter: int("quantityAfter").notNull(),
    notes: text("notes"),
    createdAt: timestamp("createdAt").defaultNow().notNull(),
  },
  table => [index("inventory_tx_product_created_idx").on(table.productId, table.createdAt), index("inventory_tx_order_idx").on(table.orderId)],
);

export const customerRequests = mysqlTable(
  "customerRequests",
  {
    id: int("id").autoincrement().primaryKey(),
    ownerId: int("ownerId").notNull().references(() => users.id, { onDelete: "cascade" }),
    customerId: int("customerId").notNull().references(() => customers.id, { onDelete: "restrict" }),
    orderId: int("orderId").references(() => orders.id, { onDelete: "set null" }),
    requestNumber: varchar("requestNumber", { length: 40 }).notNull(),
    type: mysqlEnum("type", ["change", "cancellation", "delivery", "billing", "other"]).default("other").notNull(),
    priority: mysqlEnum("priority", ["low", "medium", "high", "urgent"]).default("medium").notNull(),
    status: mysqlEnum("status", ["open", "in_progress", "resolved", "closed"]).default("open").notNull(),
    assignedTo: varchar("assignedTo", { length: 120 }),
    description: text("description").notNull(),
    resolution: text("resolution"),
    createdAt: timestamp("createdAt").defaultNow().notNull(),
    updatedAt: timestamp("updatedAt").defaultNow().onUpdateNow().notNull(),
  },
  table => [uniqueIndex("requests_owner_number_uq").on(table.ownerId, table.requestNumber), index("requests_owner_status_priority_idx").on(table.ownerId, table.status, table.priority), index("requests_customer_idx").on(table.customerId)],
);

export const requestHistory = mysqlTable(
  "requestHistory",
  {
    id: int("id").autoincrement().primaryKey(),
    ownerId: int("ownerId").notNull().references(() => users.id, { onDelete: "cascade" }),
    customerRequestId: int("customerRequestId").notNull().references(() => customerRequests.id, { onDelete: "cascade" }),
    authorName: varchar("authorName", { length: 120 }).notNull(),
    previousStatus: varchar("previousStatus", { length: 24 }),
    nextStatus: varchar("nextStatus", { length: 24 }),
    message: text("message").notNull(),
    isVisibleToCustomer: boolean("isVisibleToCustomer").default(false).notNull(),
    createdAt: timestamp("createdAt").defaultNow().notNull(),
  },
  table => [index("request_history_request_created_idx").on(table.customerRequestId, table.createdAt)],
);

export type User = typeof users.$inferSelect;
export type InsertUser = typeof users.$inferInsert;
export type Customer = typeof customers.$inferSelect;
export type Product = typeof products.$inferSelect;
export type Order = typeof orders.$inferSelect;
export type Inventory = typeof inventory.$inferSelect;
export type CustomerRequest = typeof customerRequests.$inferSelect;
