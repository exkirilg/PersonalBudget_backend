--Equal exists
CREATE OR REPLACE FUNCTION budget_items_equalexists(type INT, id INT, name VARCHAR)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
DECLARE result BOOLEAN;
BEGIN
	EXECUTE format('SELECT COUNT(id) <> 0 FROM budget_items WHERE type = %s AND id <> %s AND name ILIKE %L',
				  type, id, name) INTO result;
	RETURN result;
END $$;

--Get by id
CREATE OR REPLACE FUNCTION budget_items_getbyid(id INT)
RETURNS TABLE(
	item_id INT,
	item_name VARCHAR,
	item_type INT
)
LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY
		EXECUTE format (
			'SELECT * FROM budget_items WHERE id = %s',
			id);
END $$;

--Get all with paging
CREATE OR REPLACE FUNCTION budget_items_getallwithpaging(types INT[], page_number INT, page_size INT)
RETURNS TABLE(
	item_id INT,
	item_name VARCHAR,
	item_type INT
)
LANGUAGE plpgsql
AS $$
BEGIN
	RETURN QUERY
		EXECUTE format(
			'SELECT * FROM budget_items WHERE type = ANY (%L::int[]) ORDER BY name LIMIT %s OFFSET %s',
			types, page_size, page_size * (page_number - 1));
END $$;

--Post
CREATE OR REPLACE FUNCTION budget_items_post(name VARCHAR, type INT)
RETURNS TABLE(
	item_id INT,
	item_name VARCHAR,
	item_type INT
)
LANGUAGE plpgsql
AS $$
DECLARE result_id INT;
BEGIN
	EXECUTE format(
		'INSERT INTO budget_items (name, type) VALUES (%L, %s) RETURNING id',
		name, type) INTO result_id;
	RETURN QUERY
		EXECUTE format(
			'SELECT * FROM budget_items WHERE id = %s',
			result_id);
END $$;

--Put
CREATE OR REPLACE FUNCTION budget_items_put(id INT, name VARCHAR)
RETURNS TABLE (
	item_id INT,
	item_name VARCHAR,
	item_type INT
)
LANGUAGE plpgsql
AS $$
DECLARE result_id INT;
BEGIN
	EXECUTE format(
		'UPDATE budget_items SET name = %L WHERE id = %s RETURNING id',
		name, id) INTO result_id;
	RETURN QUERY
		EXECUTE format(
			'SELECT * FROM budget_items WHERE id = %s',
			result_id);
END $$;

--Delete
CREATE OR REPLACE FUNCTION budget_items_delete(id INT)
RETURNS BOOLEAN
LANGUAGE plpgsql
AS $$
DECLARE result_id INT;
BEGIN
	EXECUTE format(
		'DELETE FROM budget_items WHERE id = %s RETURNING id',
		id) INTO result_id;
	RETURN result_id IS NOT NULL;
END $$;
