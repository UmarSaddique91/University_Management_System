import React from 'react'


const StatCard = ({Title,Value,Change,SubText,Icon,iconBg,cardBg,iconcolor}) => {
  return (
    <div className={`${cardBg} p-3 shadow shadow-md border border-slate-300 rounded-md`}>

      {/* icon */}

        <div className='flex gap-2 '>
            <Icon className={`w-9 h-9 ${iconBg}  rounded p-2 ${iconcolor} fill-current`}/>
            <p className='text-[13px] font-semibold'>{Title}</p>
        </div>

        {/* student and percentage */}
        <div className='my-2 flex gap-3'>
          <h1 className='text-3xl font-bold'>{Value}</h1>
          <span className='text-green-700 text-[14px] font-semibold'>{Change}</span>
        </div>

        <span className='text-gray-500 text-[13px]'>{SubText}</span>

        


    </div>
  )
}

export default StatCard