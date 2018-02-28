
export type PrimitiveType = "string" | "number"  | "boolean"
export type ValueType = PrimitiveType | "date" | "enum"
export type SimpleType = PrimitiveType | "date"
export type ObjectType = "model" | "object"
export type NonCollectionType = ValueType | ObjectType
export type TypeDiscriminator = NonCollectionType | "collection"

export type Role = "value" | "primaryKey" | "foreignKey" | "referenceNavigation" | "collectionNavigation"

export interface IHaveMetadata {
    readonly $metadata: ClassType
}

export interface Domain {
    // models: { [modelName: string]: ModelMetadata },
    // externalTypes: { [modelName: string]: ExternalTypeMetadata },
    types: { [modelName: string]: ClassType }
    enums: { [modelName: string]: EnumType },
}

export interface Metadata {
    readonly type: TypeDiscriminator
    readonly name: string
    displayName: string
}


interface CustomReferenceTypeBase extends Metadata {
    readonly props: { [propName in string]: Property } 

    // The property that holds the value that represents this model.
    readonly displayProp?: Property
}
export interface ExternalType extends CustomReferenceTypeBase {
    readonly type: "object"
}
export interface ModelType extends CustomReferenceTypeBase {
    readonly type: "model"
    readonly methods: { [methodName in string]: Method }

    // We re-define this for models as non-optional
    readonly displayProp: Property

    // The primary key property.
    readonly keyProp: Property

    readonly controllerRoute: string;
}

export interface EnumValue {
    readonly strValue: string
    readonly displayName: string
    readonly value: number
}

export type EnumValues<K extends string> = 
  { [strValue in K]: EnumValue } 
& { [n: number]: EnumValue | undefined } 

export function getEnumMeta<K extends string>(values: EnumValue[]): {
    readonly valueLookup: EnumValues<K>,
    readonly values: EnumValue[]
} {
    return {
        valueLookup: {
            ...values.reduce((obj, v) => Object.assign(obj, {
                [v.strValue]: v, 
                [v.value]: v
            } as EnumValues<K>), {} as any)
        }, 
        values: values
    }
}
export interface EnumType<K extends string = string> extends Metadata {
    readonly type: "enum"
    // readonly enum: TEnum // TODO: does this belong here? Not sure. See the metadata generator for more thoughts.
    readonly valueLookup: EnumValues<K>
    readonly values: EnumValue[]
}


export type ClassType = ExternalType | ModelType 
export type CustomType = ClassType | EnumType
export type CollectableType = CustomType | SimpleType


interface PropMetaBase extends Metadata {
    readonly role: Role
}
export interface PrimitiveProperty extends PropMetaBase {
    readonly type: PrimitiveType
}
export interface DateProperty extends PropMetaBase {
    readonly type: "date"
}
export interface EnumProperty extends PropMetaBase {
    readonly type: "enum"
    readonly typeDef: EnumType
}
export interface ObjectProperty extends PropMetaBase {
    readonly type: "object"
    readonly typeDef: ExternalType
}
export interface ModelProperty extends PropMetaBase {
    readonly type: "model"
    readonly typeDef: ModelType
    readonly foreignKey: Property
}
export interface CollectionProperty extends PropMetaBase {
    readonly type: "collection"
    readonly typeDef: CollectableType
    readonly foreignKey?: Property
}
export type Property = 
  PrimitiveProperty
| DateProperty
| EnumProperty
| ObjectProperty
| ModelProperty
| CollectionProperty

export interface Method extends Metadata  {
    readonly params: Property[]
}

export type PropNames<T extends ClassType> = keyof T["props"];


