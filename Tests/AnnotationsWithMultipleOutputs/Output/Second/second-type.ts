﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated with KY.Generator 5.0.0.0
//      Manual changes to this file may cause unexpected behavior in your application.
//      Manual changes to this file will be overwritten if the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
// tslint:disable

import { SubType } from "./sub-type";

export class SecondType {
    public stringProperty: string;
    public subTypeProperty: SubType;

    public constructor(init: Partial<SecondType> = undefined) {
        Object.assign(this, init);
    }
}