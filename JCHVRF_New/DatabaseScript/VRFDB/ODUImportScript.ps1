function Fetch-AccessData{

Param( $fileName = 'C:\scripts\test.accdb', $query = 'SELECT * FROM ODU_Import_Data' )

    $conn = New-Object System.Data.OleDb.OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=$filename;Persist Security Info=False;Jet OLEDB:Database Password=VwA15CxRfN2012ThL;")
    $cmd=$conn.CreateCommand()
    $cmd.CommandText= $query
    $conn.open()
    $rdr = $cmd.ExecuteReader()
    $dt = New-Object System.Data.Datatable
    $dt.Load($rdr)
    $dt
}

function If-Found{

Param( $fileName = 'C:\scripts\test.accdb', $query = 'SELECT * FROM ODU_Import_Data' )

    $conn = New-Object System.Data.OleDb.OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=$fileName;Persist Security Info=False;Jet OLEDB:Database Password=VwA15CxRfN2012ThL;")
    $cmd=$conn.CreateCommand()
    $cmd.CommandText= $query
    $conn.open()
    $cnt = $cmd.ExecuteScalar()
    $cnt
}

function Create-NewColumns{
    $filename = "C:\JCH\VRF NextGen\Dev\VRFDesktopApplication\JCHVRF\JCHVRF_New\DB\Target\VRF.accdb"
    $dataset = Fetch-AccessData $filename "SELECT Distinct RegionCode FROM ODU_Import_Data"
    $count = $dataset.Count
    Write-Host "Total Unique Regions" $dataset.Count
    
    $index = 0
    $connUpdate = New-Object System.Data.OleDb.OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=$filename;Persist Security Info=False;Jet OLEDB:Database Password=VwA15CxRfN2012ThL;")
    $cmdUpdate=$connUpdate.CreateCommand()
    $connUpdate.open()
    $outcome = 0
    for (;$index -lt $count; $index++ ) { 

         if($dataset[$index].RegionCode -like "*/TW/*"){
              $cmdUpdate.CommandText = "ALTER TABLE ODU_Std_Universal ADD COLUMN ProductSeries TEXT(255)"
              $cmdUpdate.ExecuteNonQuery();

              $cmdUpdate.CommandText = "ALTER TABLE ODU_Std_Universal ADD COLUMN ProductCategory TEXT(255)"
              $cmdUpdate.ExecuteNonQuery();
         }
         else {
           
            # skip universal
            if($dataset[$index].RegionCode -like "*/*"){
                continue
            }

            $regionCode = $dataset[$index].RegionCode
            $cmdUpdate.CommandText = "ALTER TABLE ODU_Std_" + $regionCode + " ADD COLUMN ProductSeries TEXT(255)"

            Write-Host $cmdUpdate.CommandText
            $cmdUpdate.ExecuteNonQuery();
            
                 
            $cmdUpdate.CommandText = "ALTER TABLE ODU_Std_" + $regionCode + " ADD COLUMN ProductCategory TEXT(255)"
            
            Write-Host $cmdUpdate.CommandText
            $cmdUpdate.ExecuteNonQuery();
           
         }
         $outcome = 1
    }
    $outcome
}

function Create-NewColumnsIsInserted{
    $filename = "C:\Users\malaypa\Desktop\VRF4.1.accdb"
    $dataset = Fetch-AccessData $filename "SELECT Distinct RegionCode FROM ODU_Import_Data"
    $count = $dataset.Count
    Write-Host "Total Unique Regions" $dataset.Count
    
    $index = 0
    $connUpdate = New-Object System.Data.OleDb.OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=$filename;Persist Security Info=False;Jet OLEDB:Database Password=VwA15CxRfN2012ThL;")
    $cmdUpdate=$connUpdate.CreateCommand()
    $connUpdate.open()
    $outcome = 0
    for (;$index -lt $count; $index++ ) { 

         if($dataset[$index].RegionCode -like "*/TW/*"){
            $regionCode = $dataset[$index].RegionCode
            $cmdUpdate.CommandText = "ALTER TABLE ODU_Std_Universal ADD COLUMN IsInserted YESNO"
            $cmdUpdate.ExecuteNonQuery();
         }
         else {
            
           

            $regionCode = $dataset[$index].RegionCode
            $cmdUpdate.CommandText = "ALTER TABLE ODU_Std_" + $regionCode + " ADD COLUMN IsInserted YESNO"
            $cmdUpdate.ExecuteNonQuery();
                       
         }
         $outcome = 1
    }
    $outcome
}

function Update-Records-Universal-All-Columns{
    
    $filename = "C:\Users\malaypa\Desktop\VRF.accdb"
    $dataset = Fetch-AccessData $filename "SELECT Distinct RegionCode FROM ODU_Import_Data"
    $count = $dataset.Count
    $index = 0
    Write-Host "Total Unique Regions" $dataset.Count
    $count  = $dataset.Count
    
    $connUpdate = New-Object System.Data.OleDb.OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=$filename;Persist Security Info=False;Jet OLEDB:Database Password=VwA15CxRfN2012ThL;")
    $cmdUpdate=$connUpdate.CreateCommand()
    $connUpdate.open()
    
    for (;$index -lt $count; $index++ ) { 
        
     if($dataset[$index].RegionCode -like "*/*"){
        
        Write-Host $dataset[$index].RegionCode

                
            $cmdUpdate.CommandText = "UPDATE ODU_Std_Universal d INNER JOIN (SELECT * FROM ODU_Import_Data WHERE RegionCode = '" + $dataset[$index].RegionCode + "') s
                                        ON d.Series = s.Series
                                        AND d.Model = s.Model
                                        AND d.UnitType = s.UnitType
                                        SET 
                                            d.RegionCode=s.RegionCode,
                                            d.ProductType=s.ProductType,
                                            d.DeleteFlag=s.DeleteFlag,
                                            d.UnitType=s.UnitType,
                                            d.Model=s.Model,
                                            d.Model_York=s.Model_York,
                                            d.Model_Hitachi=s.Model_Hitachi,
                                            d.CoolCapacity=s.CoolCapacity,
                                            d.HeatCapacity=s.HeatCapacity,
                                            d.AirFlow=s.AirFlow,
                                            d.GasPipe_Hi=s.GasPipe_Hi,
                                            d.GasPipe_Lo=s.GasPipe_Lo,
                                            d.LiquidPipe=s.LiquidPipe,
                                            d.Length=s.Length,
                                            d.Width=s.Width,
                                            d.Height=s.Height,
                                            d.Power_Cooling=s.Power_Cooling,
                                            d.Power_Heating=s.Power_Heating,
                                            d.MaxCurrent=s.MaxCurrent,
                                            d.MCCB=s.MCCB,
                                            d.Weight=s.Weight,
                                            d.NoiseLevel=s.NoiseLevel,
                                            d.MaxRefrigerantCharge=s.MaxRefrigerantCharge,
                                            d.RefrigerantCharge=s.RefrigerantCharge,
                                            d.MaxIU=s.MaxIU,
                                            d.RecommendedIU=s.RecommendedIU,
                                            d.TypeImage=s.TypeImage,
                                            d.FullModuleName=s.FullModuleName,
                                            d.AuxModelName=s.AuxModelName,
                                            d.MaxPipeLength=s.MaxPipeLength,
                                            d.MaxEqPipeLength=s.MaxEqPipeLength,
                                            d.MaxOutdoorAboveHeight=s.MaxOutdoorAboveHeight,
                                            d.MaxOutdoorBelowHeight=s.MaxOutdoorBelowHeight,
                                            d.MaxDiffIndoorHeight=s.MaxDiffIndoorHeight,
                                            d.MaxIndoorLength=s.MaxIndoorLength,
                                            d.MaxIndoorLength_MaxIU=s.MaxIndoorLength_MaxIU,
                                            d.MaxPipeLengthWithFA=s.MaxPipeLengthWithFA,
                                            d.MaxDiffIndoorLength=s.MaxDiffIndoorLength,
                                            d.MaxDiffIndoorLength_MaxIU=s.MaxDiffIndoorLength_MaxIU,
                                            d.JointKitModelG=s.JointKitModelG,
                                            d.JointKitModelL=s.JointKitModelL,
                                            d.MaxOperationPI_Cooling=s.MaxOperationPI_Cooling,
                                            d.MaxOperationPI_Heating=s.MaxOperationPI_Heating,
                                            d.PartLoadTableName=s.PartLoadTableName,
                                            d.MaxTotalPipeLength=s.MaxTotalPipeLength,
                                            d.MaxTotalPipeLength_MaxIU=s.MaxTotalPipeLength_MaxIU,
                                            d.MaxMKIndoorPipeLength=s.MaxMKIndoorPipeLength,
                                            d.MaxMKIndoorPipeLength_MaxIU=s.MaxMKIndoorPipeLength_MaxIU,
                                            d.Series=s.Series,
                                            d.Horsepower=s.Horsepower,
                                            d.MinRefrigerantCharge=s.MinRefrigerantCharge,
                                            d.EER=s.EER,
                                            d.COP=s.COP,
                                            d.SEER=s.SEER,
                                            d.SCOP=s.SCOP,
                                            d.SoundPower=s.SoundPower,
                                            d.MaxCKOutdoorPipeLength=s.MaxCKOutdoorPipeLength,
                                            d.ProductSeries = [s].[Product Series],
                                            d.ProductCategory = [s].[Product Category]"

                                            
                                            Write-Host $cmdUpdate.CommandText

                                           

                                            $cnt = $cmdUpdate.ExecuteNonQuery()



                                            Write-Host $cnt

        }
      }
}

function Update-Records-Universal{
    
    $filename = "C:\JCH\VRF NextGen\Dev\VRFDesktopApplication\JCHVRF\JCHVRF_New\DB\Target\VRF.accdb"
    $dataset = Fetch-AccessData $filename "SELECT Distinct RegionCode FROM ODU_Import_Data"
    $count = $dataset.Count
    $index = 0
    Write-Host "Total Unique Regions" $dataset.Count
    $count  = $dataset.Count
    
    $connUpdate = New-Object System.Data.OleDb.OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=$filename;Persist Security Info=False;Jet OLEDB:Database Password=VwA15CxRfN2012ThL;")
    $cmdUpdate=$connUpdate.CreateCommand()
    $connUpdate.open()
    
    for (;$index -lt $count; $index++ ) { 
        
     if($dataset[$index].RegionCode -like "*/*"){
        
        Write-Host $dataset[$index].RegionCode

                
            $cmdUpdate.CommandText = "UPDATE ODU_Std_Universal d INNER JOIN (SELECT * FROM ODU_Import_Data WHERE RegionCode = '" + $dataset[$index].RegionCode + "') s
                                        ON d.Series = s.Series
                                        AND d.Model = s.Model
                                        AND d.UnitType = s.UnitType
                                        SET 
                                            
                                            d.ProductSeries = [s].[Product Series],
                                            d.ProductCategory = [s].[Product Category]"

                                            
                                            Write-Host $cmdUpdate.CommandText

                                           

                                            $cnt = $cmdUpdate.ExecuteNonQuery()



                                            Write-Host $cnt

        }
      }
}

function Update-Records-RegionsWise-All-Columns{
    
    $filename = "C:\Users\malaypa\Desktop\VRF.accdb"
    $dataset = Fetch-AccessData $filename "SELECT Distinct RegionCode FROM ODU_Import_Data"
    $count = $dataset.Count
    $index = 0
    Write-Host "Total Unique Regions" $dataset.Count
    $count  = $dataset.Count
    
    $connUpdate = New-Object System.Data.OleDb.OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=$filename;Persist Security Info=False;Jet OLEDB:Database Password=VwA15CxRfN2012ThL;")
    $cmdUpdate=$connUpdate.CreateCommand()
    $connUpdate.open()
    
    for (;$index -lt $count; $index++ ) { 
        
     if($dataset[$index].RegionCode -like "*/*"){
        
            # skip region_Code is universal
            
        }

        else{

        #if($dataset[$index].RegionCode -eq 'BGD' -or $dataset[$index].RegionCode -eq 'SEA_60Hz'){
         #   continue
        #}
            
        Write-Host $dataset[$index].RegionCode

                
            $cmdUpdate.CommandText =   "UPDATE ODU_Std_" + $dataset[$index].RegionCode +" d INNER JOIN (SELECT * FROM ODU_Import_Data WHERE RegionCode =  '" + $dataset[$index].RegionCode  +"' ) s
                                        ON d.Series = s.Series
                                        AND d.Model = s.Model
                                        AND d.UnitType = s.UnitType
                                        SET 
                                              d.RegionCode=s.RegionCode,
                                              d.ProductType=s.ProductType,
                                              d.DeleteFlag=s.DeleteFlag,
                                              d.UnitType=s.UnitType,
                                              d.Model=s.Model,
                                              d.Model_York=s.Model_York,
                                              d.Model_Hitachi=s.Model_Hitachi,
                                              d.CoolCapacity=s.CoolCapacity,
                                              d.HeatCapacity=s.HeatCapacity,
                                              d.AirFlow=s.AirFlow,
                                              d.GasPipe_Hi=s.GasPipe_Hi,
                                              d.GasPipe_Lo=s.GasPipe_Lo,
                                              d.LiquidPipe=s.LiquidPipe,
                                              d.Length=s.Length,
                                              d.Width=s.Width,
                                              d.Height=s.Height,
                                              d.Power_Cooling=s.Power_Cooling,
                                              d.Power_Heating=s.Power_Heating,
                                              d.MaxCurrent=s.MaxCurrent,
                                              d.MCCB=s.MCCB,
                                              d.Weight=s.Weight,
                                              d.NoiseLevel=s.NoiseLevel,
                                              d.MaxRefrigerantCharge=s.MaxRefrigerantCharge,
                                              d.RefrigerantCharge=s.RefrigerantCharge,
                                              d.MaxIU=s.MaxIU,
                                              d.RecommendedIU=s.RecommendedIU,
                                              d.TypeImage=s.TypeImage,
                                              d.FullModuleName=s.FullModuleName,
                                              d.AuxModelName=s.AuxModelName,
                                              d.MaxPipeLength=s.MaxPipeLength,
                                              d.MaxEqPipeLength=s.MaxEqPipeLength,
                                              d.MaxOutdoorAboveHeight=s.MaxOutdoorAboveHeight,
                                              d.MaxOutdoorBelowHeight=s.MaxOutdoorBelowHeight,
                                              d.MaxDiffIndoorHeight=s.MaxDiffIndoorHeight,
                                              d.MaxIndoorLength=s.MaxIndoorLength,
                                            d.MaxIndoorLength_MaxIU=s.MaxIndoorLength_MaxIU,
                                            d.MaxPipeLengthWithFA=s.MaxPipeLengthWithFA,
                                            d.MaxDiffIndoorLength=s.MaxDiffIndoorLength,
                                            d.MaxDiffIndoorLength_MaxIU=s.MaxDiffIndoorLength_MaxIU,
                                            d.JointKitModelG=s.JointKitModelG,
                                            d.JointKitModelL=s.JointKitModelL,
                                            d.MaxOperationPI_Cooling=s.MaxOperationPI_Cooling,
                                            d.MaxOperationPI_Heating=s.MaxOperationPI_Heating,
                                            d.PartLoadTableName=s.PartLoadTableName,
                                            d.MaxTotalPipeLength=s.MaxTotalPipeLength,
                                            d.MaxTotalPipeLength_MaxIU=s.MaxTotalPipeLength_MaxIU,
                                            d.MaxMKIndoorPipeLength=s.MaxMKIndoorPipeLength,
                                            d.MaxMKIndoorPipeLength_MaxIU=s.MaxMKIndoorPipeLength_MaxIU,
                                            d.Series=s.Series,
                                            d.Horsepower=s.Horsepower,
                                            d.ProductSeries = [s].[Product Series],
                                            d.ProductCategory = [s].[Product Category]"

                                            
                                            Write-Host $cmdUpdate.CommandText
                                            
                                            $cnt = $cmdUpdate.ExecuteNonQuery()

                                            Write-Host $cnt
        
        }

      }
}


function Update-Records-RegionsWise{
    
    $filename = "C:\JCH\VRF NextGen\Dev\VRFDesktopApplication\JCHVRF\JCHVRF_New\DB\Target\VRF.accdb"
    $dataset = Fetch-AccessData $filename "SELECT Distinct RegionCode FROM ODU_Import_Data"
    $count = $dataset.Count
    $index = 0
    Write-Host "Total Unique Regions" $dataset.Count
    $count  = $dataset.Count
    
    $connUpdate = New-Object System.Data.OleDb.OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=$filename;Persist Security Info=False;Jet OLEDB:Database Password=VwA15CxRfN2012ThL;")
    $cmdUpdate=$connUpdate.CreateCommand()
    $connUpdate.open()
    
    for (;$index -lt $count; $index++ ) { 
        
     if($dataset[$index].RegionCode -like "*/*"){
        
            # skip region_Code is universal
            
        }

        else{

        #if($dataset[$index].RegionCode -eq 'BGD' -or $dataset[$index].RegionCode -eq 'SEA_60Hz'){
         #   continue
        #}
            
        Write-Host $dataset[$index].RegionCode

                
            $cmdUpdate.CommandText =   "UPDATE ODU_Std_" + $dataset[$index].RegionCode +" d INNER JOIN (SELECT * FROM ODU_Import_Data WHERE RegionCode =  '" + $dataset[$index].RegionCode  +"' ) s
                                        ON d.Series = s.Series
                                        AND d.Model = s.Model
                                        AND d.UnitType = s.UnitType
                                        SET 
                                              
                                            d.ProductSeries = [s].[Product Series],
                                            d.ProductCategory = [s].[Product Category]"

                                            
                                            Write-Host $cmdUpdate.CommandText
                                            
                                            $cnt = $cmdUpdate.ExecuteNonQuery()

                                            Write-Host $cnt
        
        }

      }
}

function Insert-Records-Universal{
    
    $filename = "C:\Users\malaypa\Desktop\VRF4.1.accdb"
    $dataset = Fetch-AccessData $filename "SELECT Distinct RegionCode FROM ODU_Import_Data"
    $count = $dataset.Count
    $index = 0
    
    Write-Host "Total Unique Regions" $dataset.Count
    $count  = $dataset.Count
    
    $connUpdate = New-Object System.Data.OleDb.OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=$filename;Persist Security Info=False;Jet OLEDB:Database Password=VwA15CxRfN2012ThL;")
    $cmdUpdate=$connUpdate.CreateCommand()
    $connUpdate.open()
    
    for (;$index -lt $count; $index++ ) { 
        
     if($dataset[$index].RegionCode -like "*/*"){
        
        Write-Host $dataset[$index].RegionCode

                
            $cmdUpdate.CommandText = "INSERT INTO ODU_Std_Universal(RegionCode,ProductType,DeleteFlag,UnitType,Model,Model_York,Model_Hitachi,CoolCapacity,HeatCapacity,AirFlow,GasPipe_Hi,GasPipe_Lo,LiquidPipe,Length,Width,Height,Power_Cooling,Power_Heating,MaxCurrent,MCCB,Weight,NoiseLevel,MaxRefrigerantCharge,RefrigerantCharge,MaxIU,RecommendedIU,TypeImage,FullModuleName,AuxModelName,MaxPipeLength,MaxEqPipeLength,MaxOutdoorAboveHeight,MaxOutdoorBelowHeight,MaxDiffIndoorHeight,MaxIndoorLength,MaxIndoorLength_MaxIU,MaxPipeLengthWithFA,MaxDiffIndoorLength,MaxDiffIndoorLength_MaxIU,JointKitModelG,JointKitModelL,MaxOperationPI_Cooling,MaxOperationPI_Heating,PartLoadTableName,MaxTotalPipeLength,MaxTotalPipeLength_MaxIU,MaxMKIndoorPipeLength,MaxMKIndoorPipeLength_MaxIU,Series,Horsepower,MinRefrigerantCharge,EER,COP,SEER,SCOP,SoundPower,MaxCKOutdoorPipeLength,ProductSeries,ProductCategory,IsInserted)
                                      SELECT RegionCode,ProductType,DeleteFlag,UnitType,Model,Model_York,Model_Hitachi,CoolCapacity,HeatCapacity,AirFlow,GasPipe_Hi,GasPipe_Lo,LiquidPipe,Length,Width,Height,Power_Cooling,Power_Heating,MaxCurrent,MCCB,Weight,NoiseLevel,MaxRefrigerantCharge,RefrigerantCharge,MaxIU,RecommendedIU,TypeImage,FullModuleName,AuxModelName,MaxPipeLength,MaxEqPipeLength,MaxOutdoorAboveHeight,MaxOutdoorBelowHeight,MaxDiffIndoorHeight,MaxIndoorLength,MaxIndoorLength_MaxIU,MaxPipeLengthWithFA,MaxDiffIndoorLength,MaxDiffIndoorLength_MaxIU,JointKitModelG,JointKitModelL,MaxOperationPI_Cooling,MaxOperationPI_Heating,PartLoadTableName,MaxTotalPipeLength,MaxTotalPipeLength_MaxIU,MaxMKIndoorPipeLength,MaxMKIndoorPipeLength_MaxIU,Series,Horsepower,MinRefrigerantCharge,EER,COP,SEER,SCOP,SoundPower,MaxCKOutdoorPipeLength,ProductSeries,ProductCategory, IsInserted 
                                      FROM (
                                            SELECT s.*, 1 as IsInserted FROM ODU_Import_Data  s LEFT JOIN ODU_Std_Universal d 
                                                ON s.UnitType = d.UnitType AND s.Model = d.Model AND s.Series = d.Series
                                                WHERE  
                                                IsNull(d.UnitType) = -1 AND IsNull(d.Model) = -1  AND IsNull(d.Series) = -1 AND s.RegionCode = '" + $dataset[$index].RegionCode +"'
                                            )"

                                            
                                            Write-Host $cmdUpdate.CommandText
                                            
                                            $cnt = $cmdUpdate.ExecuteNonQuery()

                                            Write-Host $cnt

        }

      }

}

function Insert-Records-RegionWise{
    
    $filename = "C:\Users\malaypa\Desktop\VRF4.1.accdb"
    $dataset = Fetch-AccessData $filename "SELECT Distinct RegionCode FROM ODU_Import_Data"
    $count = $dataset.Count
    $index = 0
    
    Write-Host "Total Unique Regions" $dataset.Count
    $count  = $dataset.Count
    
    $connUpdate = New-Object System.Data.OleDb.OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=$filename;Persist Security Info=False;Jet OLEDB:Database Password=VwA15CxRfN2012ThL;")
    $cmdUpdate=$connUpdate.CreateCommand()
    $connUpdate.open()
    
    for (;$index -lt $count; $index++ ) { 
        
     if($dataset[$index].RegionCode -like "*/*"){

            Write-Host "Skipping Region"

            Write-Host $dataset[$index].RegionCode

        }
        else {
                
            #if($dataset[$index].RegionCode -eq 'BGD' -or $dataset[$index].RegionCode -eq 'SEA_60Hz'){
             #   continue
            #}
            
            #if($dataset[$index].RegionCode -ne 'THL' -and $dataset[$index].RegionCode -ne 'ME_T1'){
             #   continue
            #}

            Write-Host $dataset[$index].RegionCode
                            
            $cmdUpdate.CommandText = "INSERT INTO ODU_Std_" + $dataset[$index].RegionCode + "(RegionCode,ProductType,DeleteFlag,UnitType,Model,Model_York,Model_Hitachi,CoolCapacity,HeatCapacity,AirFlow,GasPipe_Hi,GasPipe_Lo,LiquidPipe,Length,Width,Height,Power_Cooling,Power_Heating,MaxCurrent,MCCB,Weight,NoiseLevel,MaxRefrigerantCharge,RefrigerantCharge,MaxIU,RecommendedIU,TypeImage,FullModuleName,AuxModelName,MaxPipeLength,MaxEqPipeLength,MaxOutdoorAboveHeight,MaxOutdoorBelowHeight,MaxDiffIndoorHeight,MaxIndoorLength,MaxIndoorLength_MaxIU,MaxPipeLengthWithFA,MaxDiffIndoorLength,MaxDiffIndoorLength_MaxIU,JointKitModelG,JointKitModelL,MaxOperationPI_Cooling,MaxOperationPI_Heating,PartLoadTableName,MaxTotalPipeLength,MaxTotalPipeLength_MaxIU,MaxMKIndoorPipeLength,MaxMKIndoorPipeLength_MaxIU,Series,Horsepower,ProductSeries,ProductCategory,IsInserted)
                                      SELECT RegionCode,ProductType,DeleteFlag,UnitType,Model,Model_York,Model_Hitachi,CoolCapacity,HeatCapacity,AirFlow,GasPipe_Hi,GasPipe_Lo,LiquidPipe,Length,Width,Height,Power_Cooling,Power_Heating,MaxCurrent,MCCB,Weight,NoiseLevel,MaxRefrigerantCharge,RefrigerantCharge,MaxIU,RecommendedIU,TypeImage,FullModuleName,AuxModelName,MaxPipeLength,MaxEqPipeLength,MaxOutdoorAboveHeight,MaxOutdoorBelowHeight,MaxDiffIndoorHeight,MaxIndoorLength,MaxIndoorLength_MaxIU,MaxPipeLengthWithFA,MaxDiffIndoorLength,MaxDiffIndoorLength_MaxIU,JointKitModelG,JointKitModelL,MaxOperationPI_Cooling,MaxOperationPI_Heating,PartLoadTableName,MaxTotalPipeLength,MaxTotalPipeLength_MaxIU,MaxMKIndoorPipeLength,MaxMKIndoorPipeLength_MaxIU,Series,Horsepower,ProductSeries,ProductCategory,IsInserted 
                                      FROM (
                                            SELECT s.*, 1 as IsInserted FROM ODU_Import_Data  s LEFT JOIN ODU_Std_Universal d 
                                                ON s.UnitType = d.UnitType AND s.Model = d.Model AND s.Series = d.Series
                                                WHERE  
                                                IsNull(d.UnitType) = -1 AND IsNull(d.Model) = -1  AND IsNull(d.Series) = -1 AND s.RegionCode = '" + $dataset[$index].RegionCode +"'
                                            )"
                                                                                        
                                            Write-Host $cmdUpdate.CommandText
                                            
                                            $cnt = $cmdUpdate.ExecuteNonQuery()

                                            Write-Host $cnt
        
        }

      }

}


# Import steps
##############################

# import excel data to ODU_Import_Data ##############################
# import excel data to ODU_Import_Data ##############################
# check in columns needs to be swapped ProductType <=> UnitType ################################ no need of this step as the columns are swapped

#ALTER TABLE ODU_Import_Data ADD COLUMN Tmp TEXT(255)

#UPDATE ODU_Import_Data SET Tmp = ProductType 
#WHERE RegionCode Like '*/*'

#UPDATE ODU_Import_Data SET ProductType = UnitType 
#WHERE RegionCode Like '*/*' 

#UPDATE ODU_Import_Data SET UnitType = Tmp 
#WHERE RegionCode Like '*/*'

#ALTER TABLE ODU_Import_Data DROP COLUMN Tmp

# Creating new columns ProductCategory, ProductSeries ###########################3
$result = Create-NewColumns 

# Update existing records #############################
#$result = Update-Records-Universal

# Update exting records  regionwise ######################
#$result = Update-Records-RegionsWise


# Insert records region-wise ######################
#$result =  Create-NewColumnsIsInserted

# Insert records universal
#$result = Insert-Records-Universal

# Insert records regionwise #######################
# $result = Insert-Records-RegionWise

Write-Host $result