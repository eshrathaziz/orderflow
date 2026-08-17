import { and, asc, desc, eq, inArray } from "drizzle-orm";
import { drizzle } from "drizzle-orm/mysql2";
import {
  categories,
  customerRequests,
  customers,
  inventory,
  inventoryTransactions,
  orderItems,
  orders,
  products,
  requestHistory,
  type InsertUser,
  users,
} from "../drizzle/schema";
import { ENV } from "./_core/env";

let _db: ReturnType<typeof drizzle> | null = null;

/** Lazily creates the configured managed MySQL/TiDB database connection. */
export async function getDb() {
  if (!_db && process.env.DATABASE_URL) {
    try {
      _db = drizzle(process.env.DATABASE_URL);
    } catch (error) {
      console.warn("[Database] Failed to connect:", error);
      _db = null;
    }
  }
  return _db;
}

async function requireDb() {
  const db = await getDb();
  if (!db) throw new Error("The OrderFlow database is not available.");
  return db;
}

export async function upsertUser(user: InsertUser): Promise<void> {
  if (!user.openId) throw new Error("User openId is required for upsert");
  const db = await requireDb();
  const values: InsertUser = { openId: user.openId };
  const updateSet: Record<string, unknown> = {};
  (["name", "email", "loginMethod"] as const).forEach(field => {
    if (user[field] !== undefined) {
      values[field] = user[field] ?? null;
      updateSet[field] = user[field] ?? null;
    }
  });
  values.role = user.role ?? (user.openId === ENV.ownerOpenId ? "admin" : "user");
  values.lastSignedIn = user.lastSignedIn ?? new Date();
  updateSet.role = values.role;
  updateSet.lastSignedIn = values.lastSignedIn;
  await db.insert(users).values(values).onDuplicateKeyUpdate({ set: updateSet });
}

export async function getUserByOpenId(openId: string) {
  const db = await getDb();
  if (!db) return undefined;
  const result = await db.select().from(users).where(eq(users.openId, openId)).limit(1);
  return result[0];
}

/** Creates a private, editable starter workspace only when an owner has no categories. */
export async function ensureWorkspace(ownerId: number) {
  const db = await requireDb();
  const existing = await db.select({ id: categories.id }).from(categories).where(eq(categories.ownerId, ownerId)).limit(1);
  if (existing.length) return;

  await db.transaction(async tx => {
    const networkCategory = await tx.insert(categories).values({ ownerId, name: "Network Equipment", description: "Switching and wireless infrastructure" });
    const warehouseCategory = await tx.insert(categories).values({ ownerId, name: "Warehouse Supplies", description: "Dispatch and packing supplies" });
    const networkCategoryId = Number(networkCategory[0].insertId);
    const warehouseCategoryId = Number(warehouseCategory[0].insertId);

    const customerRows = [
      { ownerId, companyName: "Northstar Logistics", contactName: "Ava Martinez", email: "ava@northstar.example", phone: "+1 415 555 0184", status: "active" as const },
      { ownerId, companyName: "Acme Health Systems", contactName: "Priya Shah", email: "priya@acmehealth.example", phone: "+1 212 555 0160", status: "active" as const },
      { ownerId, companyName: "Helios Analytics", contactName: "Nina Browne", email: "nina@helios.example", phone: "+1 312 555 0185", status: "active" as const },
    ];
    await tx.insert(customers).values(customerRows);

    const productRows = [
      { ownerId, categoryId: networkCategoryId, sku: "NET-SW-24P", name: "24-Port Managed Switch", description: "Managed Gigabit access switch", unitPrice: "1249.00", reorderLevel: 12, status: "active" as const, quantity: 8 },
      { ownerId, categoryId: networkCategoryId, sku: "NET-AP-6E", name: "Wi-Fi 6E Access Point", description: "Tri-band wireless access point", unitPrice: "399.00", reorderLevel: 20, status: "active" as const, quantity: 58 },
      { ownerId, categoryId: warehouseCategoryId, sku: "WH-THM-4X6", name: "Thermal Shipping Labels 4x6", description: "Direct thermal dispatch labels", unitPrice: "42.00", reorderLevel: 60, status: "active" as const, quantity: 38 },
      { ownerId, categoryId: warehouseCategoryId, sku: "WH-PK-VOID", name: "Void Fill Carton Pack", description: "Recycled protective void fill", unitPrice: "67.00", reorderLevel: 45, status: "active" as const, quantity: 71 },
    ];
    for (const row of productRows) {
      const { quantity, ...product } = row;
      const created = await tx.insert(products).values(product);
      const productId = Number(created[0].insertId);
      await tx.insert(inventory).values({ ownerId, productId, quantityOnHand: quantity, reservedQuantity: 0 });
      await tx.insert(inventoryTransactions).values({ ownerId, productId, type: "stock_in", quantityDelta: quantity, quantityBefore: 0, quantityAfter: quantity, notes: "Starter workspace opening balance." });
    }
  });
}

export async function getWorkspace(ownerId: number) {
  await ensureWorkspace(ownerId);
  const db = await requireDb();
  const [customerRows, productRows, orderRows, requestRows] = await Promise.all([
    db.select().from(customers).where(eq(customers.ownerId, ownerId)).orderBy(asc(customers.companyName)),
    db.select({ id: products.id, sku: products.sku, name: products.name, categoryName: categories.name, unitPrice: products.unitPrice, reorderLevel: products.reorderLevel, status: products.status, quantityOnHand: inventory.quantityOnHand, reservedQuantity: inventory.reservedQuantity })
      .from(products).innerJoin(categories, eq(products.categoryId, categories.id)).innerJoin(inventory, eq(inventory.productId, products.id)).where(eq(products.ownerId, ownerId)).orderBy(asc(products.name)),
    db.select({ id: orders.id, orderNumber: orders.orderNumber, customerName: customers.companyName, status: orders.status, totalAmount: orders.totalAmount, createdAt: orders.createdAt })
      .from(orders).innerJoin(customers, eq(orders.customerId, customers.id)).where(eq(orders.ownerId, ownerId)).orderBy(desc(orders.createdAt)),
    db.select({ id: customerRequests.id, requestNumber: customerRequests.requestNumber, customerName: customers.companyName, type: customerRequests.type, priority: customerRequests.priority, status: customerRequests.status, description: customerRequests.description, assignedTo: customerRequests.assignedTo, createdAt: customerRequests.createdAt })
      .from(customerRequests).innerJoin(customers, eq(customerRequests.customerId, customers.id)).where(eq(customerRequests.ownerId, ownerId)).orderBy(desc(customerRequests.createdAt)),
  ]);
  return { customers: customerRows, products: productRows, orders: orderRows, requests: requestRows };
}

export async function createPersistentOrder(ownerId: number, input: { customerId: number; items: { productId: number; quantity: number }[]; taxRate: number; shippingAddress?: string; notes?: string }) {
  const db = await requireDb();
  const customer = await db.select({ id: customers.id }).from(customers).where(and(eq(customers.id, input.customerId), eq(customers.ownerId, ownerId))).limit(1);
  if (!customer.length) throw new Error("The selected customer is not available in this workspace.");
  if (!input.items.length) throw new Error("At least one order line is required.");

  const productIds = Array.from(new Set(input.items.map(item => item.productId)));
  const productRows = await db.select({ id: products.id, sku: products.sku, name: products.name, unitPrice: products.unitPrice, quantityOnHand: inventory.quantityOnHand, reservedQuantity: inventory.reservedQuantity })
    .from(products).innerJoin(inventory, eq(inventory.productId, products.id)).where(and(eq(products.ownerId, ownerId), inArray(products.id, productIds)));
  if (productRows.length !== productIds.length) throw new Error("One or more selected products are unavailable.");
  const source = new Map(productRows.map(row => [row.id, row]));
  const lines = input.items.map(item => {
    const product = source.get(item.productId);
    if (!product || item.quantity < 1) throw new Error("Each order quantity must be at least one.");
    const available = product.quantityOnHand - product.reservedQuantity;
    if (item.quantity > available) throw new Error(`${product.sku} has only ${available} units available.`);
    const unitPrice = Number(product.unitPrice);
    return { ...item, product, unitPrice, lineTotal: unitPrice * item.quantity * (1 + input.taxRate) };
  });
  const subtotal = lines.reduce((sum, line) => sum + line.unitPrice * line.quantity, 0);
  const taxAmount = subtotal * input.taxRate;
  const total = subtotal + taxAmount;
  const orderNumber = `OF-${Date.now().toString(36).toUpperCase()}`;

  await db.transaction(async tx => {
    const created = await tx.insert(orders).values({ ownerId, customerId: input.customerId, orderNumber, status: "created", subtotal: subtotal.toFixed(2), taxRate: input.taxRate.toFixed(4), taxAmount: taxAmount.toFixed(2), totalAmount: total.toFixed(2), shippingAddress: input.shippingAddress || null, notes: input.notes || null });
    const orderId = Number(created[0].insertId);
    for (const line of lines) {
      await tx.insert(orderItems).values({ ownerId, orderId, productId: line.productId, skuSnapshot: line.product.sku, productNameSnapshot: line.product.name, quantity: line.quantity, unitPrice: line.unitPrice.toFixed(2), taxRate: input.taxRate.toFixed(4), lineTotal: line.lineTotal.toFixed(2) });
      const before = line.product.reservedQuantity;
      const after = before + line.quantity;
      await tx.update(inventory).set({ reservedQuantity: after }).where(eq(inventory.productId, line.productId));
      await tx.insert(inventoryTransactions).values({ ownerId, productId: line.productId, orderId, type: "reservation", quantityDelta: line.quantity, quantityBefore: before, quantityAfter: after, notes: `Reserved for ${orderNumber}.` });
    }
  });
  return { orderNumber, total };
}

export async function adjustPersistentInventory(ownerId: number, input: { productId: number; quantityDelta: number; notes?: string }) {
  const db = await requireDb();
  const row = await db.select({ id: inventory.id, productId: products.id, quantityOnHand: inventory.quantityOnHand, reservedQuantity: inventory.reservedQuantity })
    .from(inventory).innerJoin(products, eq(inventory.productId, products.id)).where(and(eq(products.ownerId, ownerId), eq(products.id, input.productId))).limit(1);
  const current = row[0];
  if (!current) throw new Error("The selected inventory record was not found.");
  const next = current.quantityOnHand + input.quantityDelta;
  if (next < current.reservedQuantity) throw new Error("The adjustment cannot reduce on-hand stock below the current reservation balance.");
  await db.transaction(async tx => {
    await tx.update(inventory).set({ quantityOnHand: next }).where(eq(inventory.id, current.id));
    await tx.insert(inventoryTransactions).values({ ownerId, productId: current.productId, type: "adjustment", quantityDelta: input.quantityDelta, quantityBefore: current.quantityOnHand, quantityAfter: next, notes: input.notes || "Inventory adjustment." });
  });
  return { quantityOnHand: next, reservedQuantity: current.reservedQuantity };
}

export const orderWorkflow = ["created", "confirmed", "processing", "shipped", "delivered", "completed"] as const;

/** Returns the only permitted forward transition, or null at the terminal state. */
export function getNextOrderStatus(status: (typeof orderWorkflow)[number]) {
  const index = orderWorkflow.indexOf(status);
  return index === -1 || index === orderWorkflow.length - 1 ? null : orderWorkflow[index + 1];
}

export async function advancePersistentOrder(ownerId: number, orderId: number) {
  const db = await requireDb();
  const found = await db.select().from(orders).where(and(eq(orders.id, orderId), eq(orders.ownerId, ownerId))).limit(1);
  const order = found[0];
  if (!order) throw new Error("The order was not found.");
  const nextStatus = getNextOrderStatus(order.status);
  if (!nextStatus) throw new Error("This order cannot advance further.");
  await db.transaction(async tx => {
    if (nextStatus === "shipped") {
      const lines = await tx.select().from(orderItems).where(eq(orderItems.orderId, orderId));
      for (const line of lines) {
        const stockRows = await tx.select().from(inventory).where(eq(inventory.productId, line.productId)).limit(1);
        const stock = stockRows[0];
        if (!stock || stock.reservedQuantity < line.quantity || stock.quantityOnHand < line.quantity) throw new Error("Stock balance changed before shipment could be processed.");
        const afterOnHand = stock.quantityOnHand - line.quantity;
        const afterReserved = stock.reservedQuantity - line.quantity;
        await tx.update(inventory).set({ quantityOnHand: afterOnHand, reservedQuantity: afterReserved }).where(eq(inventory.id, stock.id));
        await tx.insert(inventoryTransactions).values({ ownerId, productId: line.productId, orderId, type: "stock_out", quantityDelta: -line.quantity, quantityBefore: stock.quantityOnHand, quantityAfter: afterOnHand, notes: `Shipped against ${order.orderNumber}.` });
      }
    }
    await tx.update(orders).set({ status: nextStatus }).where(eq(orders.id, orderId));
  });
  return { status: nextStatus };
}

export async function createPersistentRequest(ownerId: number, input: { customerId: number; orderId?: number; type: "change" | "cancellation" | "delivery" | "billing" | "other"; priority: "low" | "medium" | "high" | "urgent"; description: string; authorName: string }) {
  const db = await requireDb();
  const customer = await db.select({ id: customers.id }).from(customers).where(and(eq(customers.id, input.customerId), eq(customers.ownerId, ownerId))).limit(1);
  if (!customer.length) throw new Error("The selected customer is not available.");
  const requestNumber = `CR-${Date.now().toString(36).toUpperCase()}`;
  const created = await db.insert(customerRequests).values({ ownerId, customerId: input.customerId, orderId: input.orderId || null, requestNumber, type: input.type, priority: input.priority, status: "open", description: input.description });
  const requestId = Number(created[0].insertId);
  await db.insert(requestHistory).values({ ownerId, customerRequestId: requestId, authorName: input.authorName, nextStatus: "open", message: input.description, isVisibleToCustomer: true });
  return { requestNumber };
}
