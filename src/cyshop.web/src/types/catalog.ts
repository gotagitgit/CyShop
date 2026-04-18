export interface CatalogType {
  id: string; // GUID as string
  name: string;
}

export interface CatalogBrand {
  id: string; // GUID as string
  name: string;
}

export interface CatalogItemDto {
  id: string; // GUID as string
  type: CatalogType;
  brand: CatalogBrand;
  name: string;
  description: string;
  price: number;
  imagePath: string;
}
